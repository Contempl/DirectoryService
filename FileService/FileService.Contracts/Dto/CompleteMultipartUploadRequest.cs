namespace FileService.Contracts.Dto;

public record CompleteMultipartUploadRequest(Guid MediaAssetId, string UploadId, List<PartETagDto> PartETags);
