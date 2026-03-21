namespace FileService.Domain.Enums;

public static class AssetTypeExtensions
{
    public static AssetType ToAssetType(this string value)
    {
        return value switch
        {
            "video" => AssetType.VIDEO,
            "preview" => AssetType.PREVIEW,
            "avatar" => AssetType.AVATAR,
            _ => throw new ArgumentOutOfRangeException($"invalid asset type: {value}"),
        };
    }
}