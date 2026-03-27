using CSharpFunctionalExtensions;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using Shared.Kernel;

namespace FileService.Domain.Assets;

public class PreviewAsset : MediaAsset
{
    public const long MAX_SIZE = 10_485_760; 
    public const string ASSET_TYPE = "preview";
    public const string LOCATION = "preview";
    public const string ALLOWED_CONTENT_TYPE = "image";
    public const string RAW_PREFIX = "raw";
    public static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];

    private PreviewAsset() { }

    private PreviewAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        MediaOwner owner,
        StorageKey key)
        : base(id, mediaData, status, AssetType.PREVIEW, owner, key)
    {
    }

    public static UnitResult<Error> ValidateForUpload(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation("preview.invalid.extension", $"File extension must be one of: {string.Join(", ", AllowedExtensions)}");
        }

        if (mediaData.ContentType.MediaType != MediaType.IMAGE)
        {
            return Error.Validation("preview.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");
        }

        if (mediaData.Size > MAX_SIZE)
        {
            return Error.Validation("preview.invalid.size", $"File size must be less than {MAX_SIZE} bytes");
        }

        return UnitResult.Success<Error>();
    }

    public static Result<PreviewAsset, Error> CreatePreviewForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        UnitResult<Error> validationResult = ValidateForUpload(mediaData);
        if (validationResult.IsFailure)
            return validationResult.Error;

        Result<StorageKey, Error> key = StorageKey.Create(LOCATION, null, id.ToString());
        if (key.IsFailure)
            return key.Error;

        var preview = new PreviewAsset(
            id,
            mediaData,
            MediaStatus.UPLOADING,
            owner,
            key.Value);

        return preview;
    }
}
