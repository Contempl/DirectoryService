namespace FileService.Contracts.Dto;

public record MediaAssetInfoResponse(
    Guid Id,
    string Status,
    string AssetType,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    FileInfoDto FileInfo,
    string? DownloadUrl);
