using CSharpFunctionalExtensions;
using FileService.Contracts.Dto;
using Shared.Kernel;

namespace FileService.Contracts;

public interface IFileCommunicationService
{
    Task<Result<MediaAssetInfoResponse, Error>> GetMediaAsset(
        Guid mediaAssetId,
        CancellationToken cancellationToken);

    Task<Result<GetMediaAssetsInfoResponse, Error>> GetMediaAssets(
        GetMediaAssetsInfoRequest request,
        CancellationToken cancellationToken);

    Task<Result<GetDownloadUrlResponse, Error>> GetDownloadUrl(
        GetDownloadUrlRequest request,
        CancellationToken cancellationToken);
}