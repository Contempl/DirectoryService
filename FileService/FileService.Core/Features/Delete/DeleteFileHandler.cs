using CSharpFunctionalExtensions;
using FileService.Core.Caching;
using FileService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.Kernel;

namespace FileService.Core.Features.Delete;

public class DeleteFileHandler
{
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<DeleteFileHandler> _logger;
    private readonly HybridCache _cache;

    public DeleteFileHandler(
        IMediaAssetsRepository mediaAssetsRepository,
        IS3Provider s3Provider,
        ILogger<DeleteFileHandler> logger,
        HybridCache cache)
    {
        _mediaAssetsRepository = mediaAssetsRepository;
        _s3Provider = s3Provider;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<Guid, Error>> Handle(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        var assetResult = await _mediaAssetsRepository.GetByIdAsync(mediaAssetId, cancellationToken);
        if (assetResult.IsFailure)
            return assetResult.Error;

        var asset = assetResult.Value;

        if (asset.Status == MediaStatus.DELETED)
            return GeneralErrors.NotFound(mediaAssetId);

        asset.MarkDeleted(DateTime.UtcNow);
        await _mediaAssetsRepository.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(FileCacheKeys.DownloadUrl(mediaAssetId), cancellationToken);

        var deleteTasks = new List<Task<Result<string, Error>>>
        {
            _s3Provider.DeleteFileAsync(asset.RawKey, cancellationToken)
        };

        if (asset.FinalKey is not null)
            deleteTasks.Add(_s3Provider.DeleteFileAsync(asset.FinalKey, cancellationToken));

        var results = await Task.WhenAll(deleteTasks);

        foreach (var r in results.Where(r => r.IsFailure))
            _logger.LogWarning("Failed to delete S3 object for asset {AssetId}: {Error}", mediaAssetId, r.Error.Message);

        _logger.LogInformation("Deleted asset {AssetId}", mediaAssetId);
        return mediaAssetId;
    }
}
