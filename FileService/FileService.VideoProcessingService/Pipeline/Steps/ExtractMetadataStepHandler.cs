using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class ExtractMetadataStepHandler : IProcessingStepHandler
{
    private readonly IFfmpegProcessRunner _ffmpegProcessRunner;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<ExtractMetadataStepHandler> _logger;

    public ExtractMetadataStepHandler(
        IFfmpegProcessRunner ffmpegProcessRunner,
        IS3Provider s3Provider,
        ILogger<ExtractMetadataStepHandler> logger)
    {
        _ffmpegProcessRunner = ffmpegProcessRunner;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public StepType StepType => StepType.EXTRACT_METADATA;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Extracting metadata for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        var inputFileUrlResult = await _s3Provider.DownloadFileAsync(
            context.VideoAsset.UploadKey,
            cancellationToken);

        if (inputFileUrlResult.IsFailure)
            return inputFileUrlResult.Error;

        context.SetMediaAssetUrl(inputFileUrlResult.Value);

        var metadataResult = await _ffmpegProcessRunner.ExtractMetadataAsync(
            inputFileUrlResult.Value,
            cancellationToken);

        if (metadataResult.IsFailure)
            return metadataResult.Error;

        context.VideoAsset.SetMetadata(metadataResult.Value);

        return context;
    }
}
