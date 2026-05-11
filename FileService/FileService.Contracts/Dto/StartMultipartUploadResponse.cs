namespace FileService.Contracts.Dto;

public record StartMultipartUploadResponse(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<ChunkUploadUrl> ChunkUrls,
    long ChunkSize);
