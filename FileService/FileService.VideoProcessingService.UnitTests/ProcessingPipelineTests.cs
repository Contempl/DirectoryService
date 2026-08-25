using System.Data;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.Assets;
using FileService.Domain.Enums;
using FileService.Domain.MediaProcessing;
using FileService.Domain.ValueObjects;
using FileService.VideoProcessing.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Kernel;
using VideoProcessingEntity = FileService.Domain.MediaProcessing.VideoProcessing;

namespace FileService.VideoProcessingService.UnitTests;

public class ProcessingPipelineTests
{
    private static readonly StepType[] StepOrder =
    [
        StepType.INITIALIZE,
        StepType.EXTRACT_METADATA,
        StepType.GENERATE_HLS,
        StepType.UPLOAD_HLS,
        StepType.GENERATE_PREVIEW,
        StepType.CLEANUP
    ];

    [Fact]
    public async Task ProcessAllStepsAsync_ShouldExecuteStepsInOrderAndCompleteVideo()
    {
        //Arrange
        var videoAsset = CreateUploadedVideoAsset();
        var executedSteps = new List<StepType>();
        var handlers = StepOrder
            .Select(stepType => new RecordingStepHandler(stepType, executedSteps))
            .Cast<IProcessingStepHandler>()
            .ToArray();
        var fixture = CreatePipeline(videoAsset, handlers);

        //Act
        var result = await fixture.Pipeline.ProcessAllStepsAsync(videoAsset.Id);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StepOrder, executedSteps);
        Assert.Equal(ProcessingStatus.COMPLETED, fixture.VideoProcessingRepository.VideoProcessing!.Status);
        Assert.All(
            fixture.VideoProcessingRepository.VideoProcessing.Steps,
            step => Assert.Equal(StepStatus.COMPLETED, step.Status));
        Assert.Equal(MediaStatus.READY, videoAsset.Status);
        Assert.Equal(100, fixture.VideoProcessingRepository.VideoProcessing.ProgressPercentage);
        Assert.Equal("master.m3u8", videoAsset.FinalKey.Key);
    }

    [Fact]
    public async Task ProcessAllStepsAsync_WhenMiddleStepFails_ShouldStopAndMarkVideoAsFailed()
    {
        //Arrange
        var videoAsset = CreateUploadedVideoAsset();
        var executedSteps = new List<StepType>();
        var handlers = StepOrder
            .Select(stepType => new RecordingStepHandler(
                stepType,
                executedSteps,
                shouldFail: stepType == StepType.GENERATE_HLS))
            .Cast<IProcessingStepHandler>()
            .ToArray();
        var fixture = CreatePipeline(videoAsset, handlers);

        //Act
        var result = await fixture.Pipeline.ProcessAllStepsAsync(videoAsset.Id);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            [StepType.INITIALIZE, StepType.EXTRACT_METADATA, StepType.GENERATE_HLS],
            executedSteps);
        Assert.Equal(ProcessingStatus.FAILED, fixture.VideoProcessingRepository.VideoProcessing!.Status);
        Assert.Equal(MediaStatus.FAILED, videoAsset.Status);
        Assert.Equal(
            StepStatus.FAILED,
            fixture.VideoProcessingRepository.VideoProcessing.Steps
                .Single(step => step.Type == StepType.GENERATE_HLS)
                .Status);
        Assert.Equal(
            StepStatus.PENDING,
            fixture.VideoProcessingRepository.VideoProcessing.Steps
                .Single(step => step.Type == StepType.UPLOAD_HLS)
                .Status);
    }

    [Fact]
    public async Task ProcessAllStepsAsync_WhenCancelled_ShouldStopAndMarkVideoAsFailed()
    {
        //Arrange
        var videoAsset = CreateUploadedVideoAsset();
        var executedSteps = new List<StepType>();
        var handlers = StepOrder
            .Select(stepType => new RecordingStepHandler(stepType, executedSteps))
            .Cast<IProcessingStepHandler>()
            .ToArray();
        var fixture = CreatePipeline(videoAsset, handlers);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        //Act
        var result = await fixture.Pipeline.ProcessAllStepsAsync(
            videoAsset.Id,
            cancellationTokenSource.Token);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal("pipeline.step.cancelled", result.Error.Code);
        Assert.Single(executedSteps);
        Assert.Equal(StepType.INITIALIZE, executedSteps[0]);
        Assert.Equal(ProcessingStatus.FAILED, fixture.VideoProcessingRepository.VideoProcessing!.Status);
        Assert.Equal(MediaStatus.FAILED, videoAsset.Status);
        Assert.Contains(
            fixture.VideoProcessingRepository.VideoProcessing.Steps,
            step => step.Status == StepStatus.FAILED);
    }

    [Fact]
    public async Task ProcessAllStepsAsync_WhenStartedAgainAfterCompletion_ShouldReturnFailure()
    {
        //Arrange
        var videoAsset = CreateUploadedVideoAsset();
        var executedSteps = new List<StepType>();
        var handlers = StepOrder
            .Select(stepType => new RecordingStepHandler(stepType, executedSteps))
            .Cast<IProcessingStepHandler>()
            .ToArray();
        var fixture = CreatePipeline(videoAsset, handlers);
        var firstResult = await fixture.Pipeline.ProcessAllStepsAsync(videoAsset.Id);
        executedSteps.Clear();

        //Act
        var secondResult = await fixture.Pipeline.ProcessAllStepsAsync(videoAsset.Id);

        //Assert
        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsFailure);
        Assert.Empty(executedSteps);
        Assert.Equal(MediaStatus.READY, videoAsset.Status);
        Assert.Equal(ProcessingStatus.COMPLETED, fixture.VideoProcessingRepository.VideoProcessing!.Status);
    }

    private static PipelineFixture CreatePipeline(
        VideoAsset videoAsset,
        IEnumerable<IProcessingStepHandler> handlers)
    {
        var videoProcessingRepository = new FakeVideoProcessingRepository();
        var mediaAssetsRepository = new FakeMediaAssetsRepository(videoAsset);
        var transactionManager = new FakeTransactionManager();

        var pipeline = new ProcessingPipeline(
            handlers,
            NullLogger<ProcessingPipeline>.Instance,
            videoProcessingRepository,
            mediaAssetsRepository,
            transactionManager);

        return new PipelineFixture(pipeline, videoProcessingRepository);
    }

    private static VideoAsset CreateUploadedVideoAsset()
    {
        var fileName = FileName.Create("video.mp4").Value;
        var contentType = ContentType.Create("video/mp4").Value;
        var mediaData = MediaData.Create(fileName, contentType, 1024, 1).Value;
        var owner = MediaOwner.ForUser(Guid.NewGuid()).Value;
        var videoAsset = VideoAsset.CreateMediaForUpload(Guid.NewGuid(), mediaData, owner).Value;
        videoAsset.MarkUploaded(DateTime.UtcNow);

        return videoAsset;
    }

    private sealed record PipelineFixture(
        ProcessingPipeline Pipeline,
        FakeVideoProcessingRepository VideoProcessingRepository);

    private sealed class RecordingStepHandler : IProcessingStepHandler
    {
        private readonly List<StepType> _executedSteps;
        private readonly bool _shouldFail;

        public RecordingStepHandler(
            StepType stepType,
            List<StepType> executedSteps,
            bool shouldFail = false)
        {
            StepType = stepType;
            _executedSteps = executedSteps;
            _shouldFail = shouldFail;
        }

        public StepType StepType { get; }

        public Task<Result<ProcessingContext, Error>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken = default)
        {
            _executedSteps.Add(StepType);
            cancellationToken.ThrowIfCancellationRequested();

            if (_shouldFail)
            {
                return Task.FromResult<Result<ProcessingContext, Error>>(
                    Error.Failure("pipeline.test.failure", "Test step failed."));
            }

            return Task.FromResult(Result.Success<ProcessingContext, Error>(context));
        }
    }

    private sealed class FakeVideoProcessingRepository : IVideoProcessingRepository
    {
        public VideoProcessingEntity? VideoProcessing { get; private set; }

        public Task<Result<VideoProcessingEntity, Error>> GetByAsync(
            Expression<Func<VideoProcessingEntity, bool>> predicate,
            CancellationToken cancellationToken)
        {
            if (VideoProcessing is not null && predicate.Compile()(VideoProcessing))
                return Task.FromResult<Result<VideoProcessingEntity, Error>>(VideoProcessing);

            return Task.FromResult<Result<VideoProcessingEntity, Error>>(GeneralErrors.NotFound());
        }

        public Result<Guid, Error> Add(VideoProcessingEntity videoProcessing)
        {
            VideoProcessing = videoProcessing;
            return videoProcessing.Id;
        }
    }

    private sealed class FakeMediaAssetsRepository : IMediaAssetsRepository
    {
        private readonly VideoAsset _videoAsset;

        public FakeMediaAssetsRepository(VideoAsset videoAsset)
        {
            _videoAsset = videoAsset;
        }

        public UnitResult<Error> Add(
            MediaAsset mediaAsset,
            CancellationToken cancellationToken = default) => UnitResult.Success<Error>();

        public Task<Result<MediaAsset, Error>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Result<MediaAsset, Error>>(_videoAsset);

        public Task<Result<VideoAsset, Error>> GetVideoBy(
            Expression<Func<VideoAsset, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            if (predicate.Compile()(_videoAsset))
                return Task.FromResult<Result<VideoAsset, Error>>(_videoAsset);

            return Task.FromResult<Result<VideoAsset, Error>>(GeneralErrors.NotFound());
        }

        public Task<IReadOnlyList<MediaAsset>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<MediaAsset>>([_videoAsset]);

        public Task<UnitResult<Error>> RemoveAsync(
            MediaAsset mediaAsset,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(UnitResult.Success<Error>());

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeTransactionManager : ITransactionManager
    {
        public Task<Result<int, Error>> SaveChangesAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success<int, Error>(1));

        public Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
