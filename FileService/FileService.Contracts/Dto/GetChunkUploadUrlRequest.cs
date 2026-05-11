namespace FileService.Contracts.Dto;

public record GetChunkUploadUrlRequest(Guid MediaAssetId, string UploadId, int PartNumber);
