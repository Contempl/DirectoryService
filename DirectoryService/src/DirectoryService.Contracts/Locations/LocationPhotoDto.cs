namespace DirectoryService.Contracts.Locations;

public sealed record LocationPhotoDto
{
    public required Guid AssetId { get; init; }

    public required string Status { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required long Size { get; init; }

    public required DateTime VerifiedAt { get; init; }

    public string? ContentUrl { get; init; }
}
