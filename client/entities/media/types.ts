export type StartMultipartUploadRequest = {
  fileName: string;
  assetType: string;
  contentType: string;
  size: number;
  context: string;
  contextId: string;
};

export type ChunkUploadUrl = {
  partNumber: number;
  uploadUrl: string;
};

export type StartMultipartUploadResponse = {
  mediaAssetId: string;
  uploadId: string;
  chunkUrls: ChunkUploadUrl[];
  chunkSize: number;
};

export type PartETag = {
  partNumber: number;
  eTag: string;
};

export type CompleteMultipartUploadRequest = {
  mediaAssetId: string;
  uploadId: string;
  partETags: PartETag[];
};

export type CompleteMultipartUploadResponse = {
  mediaAssetId: string;
};

export type CancelMultipartUploadRequest = {
  mediaAssetId: string;
  uploadId: string;
};
