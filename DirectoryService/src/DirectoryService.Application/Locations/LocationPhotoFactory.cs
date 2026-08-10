using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using Shared.Kernel;
using FileService.Contracts.Dto;

namespace DirectoryService.Application.Locations;

internal static class LocationPhotoFactory
{
    public static Result<LocationPhoto, Error> Create(MediaAssetInfoResponse asset)
    {
        if (!string.Equals(asset.Status, "ready", StringComparison.OrdinalIgnoreCase))
            return LocationPhotoErrors.AssetNotReady();

        if (!string.Equals(asset.AssetType, "preview", StringComparison.OrdinalIgnoreCase))
            return LocationPhotoErrors.InvalidAssetType();

        if (!asset.FileInfo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return LocationPhotoErrors.InvalidContentType();

        return LocationPhoto.Create(
            asset.Id,
            asset.FileInfo.FileName,
            asset.FileInfo.ContentType,
            asset.FileInfo.Size,
            DateTime.UtcNow);
    }
}
