using CSharpFunctionalExtensions;
using FileService.Domain.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class GenerateHlsStepHandler : IProcessingStepHandler
{
    private readonly IFfmpegProcessRunner _ffmpegProcessRunner;
    private readonly ILogger<GenerateHlsStepHandler> _logger;

    public GenerateHlsStepHandler(
        IFfmpegProcessRunner ffmpegProcessRunner,
        ILogger<GenerateHlsStepHandler> logger)
    {
        _ffmpegProcessRunner = ffmpegProcessRunner;
        _logger = logger;
    }

    public StepType StepType => StepType.GENERATE_HLS;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(context.WorkingDirectory) ||
            string.IsNullOrWhiteSpace(context.HlsOutputDirectory))
        {
            return Error.Failure(
                "pipeline.working.directory.missing",
                "Working directory must be initialized before generating HLS output.");
        }

        if (string.IsNullOrWhiteSpace(context.MediaAssetUrl))
        {
            return Error.Failure(
                "pipeline.media.asset.url.missing",
                "Media asset URL must be initialized before generating HLS output.");
        }

        _logger.LogInformation(
            "Generating HLS for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        if (context.VideoAsset.Metadata is null)
        {
            _logger.LogWarning(
                "Video metadata is missing for VideoAssetId: {VideoAssetId}. Progress tracking will be disabled.",
                context.VideoAsset.Id);
        }

        var generationResult = await _ffmpegProcessRunner.GenerateHlsAsync(
            context.MediaAssetUrl!,
            context.HlsOutputDirectory!,
            cancellationToken);

        if (generationResult.IsFailure)
            return generationResult.Error;

        return context;
    }
}
