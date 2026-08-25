using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain;
using FileService.Domain.MediaProcessing;
using FileService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Kernel;

namespace FileService.VideoProcessing.Pipeline.Steps;

public sealed class UploadHlsStepHandler : IProcessingStepHandler
{
    private readonly IS3Provider _s3Provider;
    private readonly VideoProcessingOptions _options;
    private readonly ILogger<UploadHlsStepHandler> _logger;

    public UploadHlsStepHandler(
        IS3Provider s3Provider,
        IOptions<VideoProcessingOptions> options,
        ILogger<UploadHlsStepHandler> logger)
    {
        _s3Provider = s3Provider;
        _options = options.Value;
        _logger = logger;
    }

    public StepType StepType => StepType.UPLOAD_HLS;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Uploading HLS to S3 for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        if (string.IsNullOrWhiteSpace(context.HlsOutputDirectory))
            return FileErrors.HlsProcessingFailed("HLS output directory is not set");

        if (!Directory.Exists(context.HlsOutputDirectory))
            return FileErrors.HlsProcessingFailed("HLS output directory does not exist");

        string[] hlsFiles = Directory.GetFiles(
            context.HlsOutputDirectory,
            "*.*",
            SearchOption.AllDirectories);

        if (hlsFiles.Length == 0)
            return FileErrors.HlsProcessingFailed("No HLS files found in output directory");

        var hlsRootKey = context.VideoAsset.GetHlsRootKey();
        if (hlsRootKey.IsFailure)
            return hlsRootKey.Error;

        using var throttler = new SemaphoreSlim(_options.UploadDegreeOfParallelism);

        Task<UnitResult<Error>>[] uploadTasks = hlsFiles.Select(async file =>
        {
            await throttler.WaitAsync(cancellationToken);
            try
            {
                return await UploadHlsFileAsync(
                    hlsRootKey.Value,
                    file,
                    cancellationToken);
            }
            finally
            {
                throttler.Release();
            }
        }).ToArray();

        UnitResult<Error>[] results = await Task.WhenAll(uploadTasks);

        UnitResult<Error> firstFailure = results.FirstOrDefault(result => result.IsFailure);
        if (firstFailure.IsFailure)
            return firstFailure.Error;

        _logger.LogInformation(
            "Successfully uploaded {FileCount} HLS files for VideoAssetId: {VideoAssetId}",
            hlsFiles.Length,
            context.VideoAsset.Id);

        var masterPlaylistKey = context.VideoAsset.GetHlsMasterPlaylistKey();
        if (masterPlaylistKey.IsFailure)
            return masterPlaylistKey.Error;

        var setKeyResult = context.VideoAsset.SetHlsMasterPlaylistKey(masterPlaylistKey.Value);
        if (setKeyResult.IsFailure)
            return setKeyResult.Error;

        return context;
    }

    private async Task<UnitResult<Error>> UploadHlsFileAsync(
        StorageKey hlsRootKey,
        string localFilePath,
        CancellationToken cancellationToken)
    {
        string fileName = Path.GetFileName(localFilePath);

        var storageKey = hlsRootKey.AppendSegment(fileName);
        if (storageKey.IsFailure)
            return storageKey.Error;

        string contentType = GetContentType(localFilePath);

        await using FileStream fileStream = File.OpenRead(localFilePath);

        return await _s3Provider.UploadFileAsync(
            storageKey.Value,
            fileStream,
            contentType,
            cancellationToken);
    }

    private static string GetContentType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".m3u8" => "application/vnd.apple.mpegurl",
            ".ts" => "video/mp2t",
            _ => "application/octet-stream"
        };
    }
}
