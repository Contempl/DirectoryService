using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dto;
using Shared.Kernel;

namespace DirectoryService.IntegrationTests;

public sealed class FakeFileCommunicationService : IFileCommunicationService
{
    private readonly Dictionary<Guid, MediaAssetInfoResponse> _assets = [];

    public bool IsUnavailable { get; set; }

    public void Add(MediaAssetInfoResponse asset) => _assets[asset.Id] = asset;

    public void Remove(Guid assetId) => _assets.Remove(assetId);

    public void Reset()
    {
        _assets.Clear();
        IsUnavailable = false;
    }

    public Task<Result<MediaAssetInfoResponse, Error>> GetMediaAsset(
        Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        if (IsUnavailable)
            return Task.FromResult(Result.Failure<MediaAssetInfoResponse, Error>(Unavailable()));

        return Task.FromResult(_assets.TryGetValue(mediaAssetId, out var asset)
                               && !string.Equals(asset.Status, "deleted", StringComparison.OrdinalIgnoreCase)
            ? Result.Success<MediaAssetInfoResponse, Error>(asset)
            : Result.Failure<MediaAssetInfoResponse, Error>(
                Error.NotFound("media.asset.not.found", "Media asset was not found.")));
    }

    public Task<Result<GetMediaAssetsInfoResponse, Error>> GetMediaAssets(
        GetMediaAssetsInfoRequest request,
        CancellationToken cancellationToken)
    {
        if (IsUnavailable)
            return Task.FromResult(Result.Failure<GetMediaAssetsInfoResponse, Error>(Unavailable()));

        var assets = request.MediaAssetIds
            .Where(_assets.ContainsKey)
            .Select(id => _assets[id])
            .Where(asset => !string.Equals(asset.Status, "deleted", StringComparison.OrdinalIgnoreCase))
            .Select(asset => new MediaAssetBriefDto(asset.Id, asset.Status, asset.DownloadUrl))
            .ToList();

        return Task.FromResult(Result.Success<GetMediaAssetsInfoResponse, Error>(
            new GetMediaAssetsInfoResponse(assets)));
    }

    public Task<Result<GetDownloadUrlResponse, Error>> GetDownloadUrl(
        GetDownloadUrlRequest request,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    private static Error Unavailable() => Error.Failure(
        "file-service.unavailable",
        "File Service is temporarily unavailable.");
}
