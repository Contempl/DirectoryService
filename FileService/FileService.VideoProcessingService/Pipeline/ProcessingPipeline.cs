using CSharpFunctionalExtensions;
using FileService.Core;
using Microsoft.Extensions.Logging;
using Shared.Kernel;
using VideoProcessingEntity = FileService.Domain.MediaProcessing.VideoProcessing;

namespace FileService.VideoProcessing.Pipeline;

public class ProcessingPipeline : IProcessingPipeline
{
    private readonly IEnumerable<IProcessingStepHandler> _stepHandlers;
    private readonly ILogger<ProcessingPipeline> _logger;
    private readonly IVideoProcessingRepository _videoProcessingRepository;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly ITransactionManager _transactionManager;

    public ProcessingPipeline(IEnumerable<IProcessingStepHandler> stepHandlers,
        ILogger<ProcessingPipeline> logger,
        IVideoProcessingRepository videoProcessingRepository,
        IMediaAssetsRepository mediaAssetsRepository,
        ITransactionManager transactionManager)
    {
        _stepHandlers = stepHandlers;
        _logger = logger;
        _videoProcessingRepository = videoProcessingRepository;
        _mediaAssetsRepository = mediaAssetsRepository;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Error>> ProcessAllStepsAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        var contextResult = await LoadContextAsync(videoAssetId, cancellationToken);
        if (contextResult.IsFailure)
            return contextResult.Error;

        var context = contextResult.Value;
        
        while (true)
        {
            var stepResult = context.VideoProcessing.ProcessNextStep();
            
            if (stepResult.IsFailure)
                return stepResult.Error;
            
            if (stepResult.Value is null)
            {
                var assetCompleteResult = context.VideoAsset.CompleteProcessing(DateTime.UtcNow);
                if (assetCompleteResult.IsFailure)
                    return assetCompleteResult.Error;

                var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                    return saveResult.Error;

                return UnitResult.Success<Error>();
            }
            
            var currentStep = stepResult.Value;
            
            var stepHandler = _stepHandlers.FirstOrDefault(s => s.StepType == currentStep.Type);
            if (stepHandler is null)
            {
                // код на ошибку
                context.VideoProcessing.FailCurrentStep("No handler found for step type");
                context.VideoAsset.MarkFailed(DateTime.UtcNow);
                var savedResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                return Error.Failure("pipeline.handler.not.found", "Not handler for step type");
            }

            var executeResult = await ExecuteStepSafelyAsync(
                stepHandler,
                context,
                cancellationToken);

            if (executeResult.IsFailure)
            {
                var failResult = context.VideoProcessing.FailCurrentStep(executeResult.Error.Message);
                if (failResult.IsFailure)
                    return failResult.Error;

                var assetFailResult = context.VideoAsset.MarkFailed(DateTime.UtcNow);
                if (assetFailResult.IsFailure)
                    return assetFailResult.Error;

                var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                    return saveResult.Error;

                return executeResult.Error;
            }

            context = executeResult.Value;

            var completeResult = context.VideoProcessing.CompleteCurrentStep();
            if (completeResult.IsFailure)
                return completeResult.Error;

            var stepSaveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (stepSaveResult.IsFailure)
                return stepSaveResult.Error;
        }
    }

    private async Task<Result<ProcessingContext, Error>> ExecuteStepSafelyAsync(
        IProcessingStepHandler handler,
        ProcessingContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            return await handler.ExecuteAsync(context, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled error while executing video processing step {StepType}",
                handler.StepType);

            return Error.Failure(
                "pipeline.step.execution.failed",
                exception.Message);
        }
    }

    private async Task<Result<ProcessingContext, Error>> LoadContextAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken)
    {
        var processingResult = await _videoProcessingRepository
            .GetByAsync(vp => vp.VideoAssetId == videoAssetId, cancellationToken);

        VideoProcessingEntity videoProcess;

        if (processingResult.IsFailure)
        {
            var newProcess = new VideoProcessingEntity(videoAssetId);
            videoProcess = newProcess;
            
            _videoProcessingRepository.Add(videoProcess);
            
            _logger.LogInformation("Created video process for VideoAsset: {VideoAssetId}", videoAssetId);
        }
        else
        {
            videoProcess = processingResult.Value;
            _logger.LogInformation("Loaded existing VideoProcess for VideoAsset: {VideoAssetId}", videoAssetId);
        }

        var assetResult = await _mediaAssetsRepository
            .GetVideoBy(a => a.Id == videoAssetId, cancellationToken);
        
        if (assetResult.IsFailure)
            return assetResult.Error;

        var startResult = assetResult.Value.StartProcessing();
        if (startResult.IsFailure)
            return startResult.Error;

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error;

        ProcessingContext processingContext = new ProcessingContext
        {
            VideoAsset = assetResult.Value,
            VideoProcessing = videoProcess
        };

        return processingContext;
    }
}
