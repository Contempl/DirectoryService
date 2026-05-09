namespace FileService.Contracts.Dto;

public record CancelMultipartUploadRequest(Guid MediaAssetId, string UploadId);
