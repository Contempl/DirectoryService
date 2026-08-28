using CSharpFunctionalExtensions;
using FileService.VideoProcessing.Pipeline;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.VideoProcessing;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<VideoProcessingService> _logger;
    private readonly IProcessingPipeline _processingPipeline;

    public VideoProcessingService(
        ILogger<VideoProcessingService> logger,
        IProcessingPipeline processingPipeline)
    {
        _logger = logger;
        _processingPipeline = processingPipeline;
    }

    public async Task<UnitResult<Error>> ProcessVideoAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting video processing for VideoAsset: {VideoAssetId}", videoAssetId);

        var result = await _processingPipeline.ProcessAllStepsAsync(videoAssetId, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogError(
                "Video processing failed for VideoAsset: {VideoAssetId}. Error: {Error}",
                videoAssetId,
                result.Error.Message);

            return result.Error;
        }

        _logger.LogInformation("Video processing completed for VideoAsset: {VideoAssetId}", videoAssetId);

        return UnitResult.Success<Error>();
    }
}
