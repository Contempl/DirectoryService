using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain.MediaProcessing;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class CleanupStepHandler : IProcessingStepHandler
{
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<CleanupStepHandler> _logger;

    public CleanupStepHandler(
        IS3Provider s3Provider,
        ILogger<CleanupStepHandler> logger)
    {
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public StepType StepType => StepType.CLEANUP;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Cleaning up temporary files for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        if (string.IsNullOrWhiteSpace(context.WorkingDirectory))
        {
            _logger.LogWarning("Working directory is not set, skipping cleanup");
            return context;
        }

        var deleteResult = await _s3Provider.DeleteFileAsync(
            context.VideoAsset.RawKey,
            cancellationToken);

        if (deleteResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to delete raw file from storage for VideoAssetId: {VideoAssetId}. Error: {Error}",
                context.VideoAsset.Id,
                deleteResult.Error);
        }
        else
        {
            _logger.LogDebug(
                "Raw file deleted from storage for VideoAssetId: {VideoAssetId}",
                context.VideoAsset.Id);
        }

        string workingDirectory = context.WorkingDirectory;
        try
        {
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, recursive: true);
                _logger.LogDebug(
                    "Working directory deleted: {WorkingDirectory}",
                    workingDirectory);

                context.Cleanup();
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to delete working directory: {WorkingDirectory}. Will be cleaned up later.",
                workingDirectory);
        }

        return context;
    }
}
