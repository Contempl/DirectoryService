using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace DirectoryService.Domain.Entities;

public sealed class LocationPhoto
{
    private LocationPhoto() { }

    private LocationPhoto(
        Guid assetId,
        string fileName,
        string contentType,
        long size,
        DateTime verifiedAt)
    {
        AssetId = assetId;
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        VerifiedAt = verifiedAt;
    }

    public Guid AssetId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long Size { get; private set; }

    public DateTime VerifiedAt { get; private set; }

    public static Result<LocationPhoto, Error> Create(
        Guid assetId,
        string fileName,
        string contentType,
        long size,
        DateTime verifiedAt)
    {
        if (assetId == Guid.Empty)
            return LocationPhotoErrors.AssetIdRequired();

        if (string.IsNullOrWhiteSpace(fileName))
            return LocationPhotoErrors.FileNameRequired();

        if (string.IsNullOrWhiteSpace(contentType))
            return LocationPhotoErrors.ContentTypeRequired();

        if (size < 0)
            return LocationPhotoErrors.SizeInvalid();

        return new LocationPhoto(assetId, fileName, contentType, size, verifiedAt);
    }
}
