namespace FileService.Contracts.Dto;

public record ChunkUploadUrl()
{
    public int PartNumber { get; init; }

    public string UploadUrl { get; init; } = string.Empty;
}