using Shared.Kernel;

namespace DirectoryService.Domain.Entities;

public static class LocationPhotoErrors
{
    public static Error AssetIdRequired() => Error.Validation(
        "location.photo.asset_id.required",
        "Photo asset id is required.");

    public static Error FileNameRequired() => Error.Validation(
        "location.photo.file_name.invalid",
        "Photo file name is required.");

    public static Error ContentTypeRequired() => Error.Validation(
        "location.photo.content_type.invalid",
        "Photo content type is required.");

    public static Error SizeInvalid() => Error.Validation(
        "location.photo.size.invalid",
        "Photo size cannot be negative.");

    public static Error AlreadyAttached() => Error.Conflict(
        "location.photo.already_attached",
        "Location already has a photo. Use replace instead.");

    public static Error NotAttached() => Error.NotFound(
        "location.photo.not_found",
        "Location photo is not attached.");

    public static Error AssetUnchanged() => Error.Conflict(
        "location.photo.asset_unchanged",
        "The specified asset is already attached to this location.");

    public static Error AssetNotReady() => Error.Conflict(
        "location.photo.asset_not_ready",
        "Photo asset must be ready before it can be attached.");

    public static Error InvalidAssetType() => Error.Validation(
        "location.photo.asset_type.invalid",
        "Only preview assets can be attached as location photos.",
        "AssetType");

    public static Error InvalidContentType() => Error.Validation(
        "location.photo.content_type.invalid",
        "Only image assets can be attached as location photos.",
        "ContentType");
}
