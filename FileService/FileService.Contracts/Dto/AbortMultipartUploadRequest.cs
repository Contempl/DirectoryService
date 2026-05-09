namespace FileService.Contracts.Dto;

public record AbortMultipartUploadRequest(Guid MediaAssetId, string UploadId);
