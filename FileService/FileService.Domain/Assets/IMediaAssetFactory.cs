using CSharpFunctionalExtensions;
using FileService.Domain.ValueObjects;
using Shared.Kernel;

namespace FileService.Domain.Assets;

public interface IMediaAssetFactory
{
    Result<VideoAsset, Error> CreateMediaForUpload(Guid id, MediaData mediaData, MediaOwner owner);
    Result<PreviewAsset, Error> CreatePreviewForUpload(MediaData mediaData, MediaOwner owner);
}