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
  headers?: Record<string, string>;
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

export type CancelMultipartUploadResponse = {
  success: boolean;
};

export type MediaFileInfo = {
  fileName: string;
  contentType: string;
  size: number;
};

export type MediaAssetInfo = {
  id: string;
  status: string;
  assetType: string;
  createdAt: string;
  updatedAt: string;
  fileInfo: MediaFileInfo;
  downloadUrl: string | null;
};

export type VideoProcessingStatus = {
  videoAssetId: string;
  status: string;
  currentStep: string | null;
  progressPercentage: number;
  errorMessage: string | null;
  startedAt: string | null;
  completedAt: string | null;
  isTerminal: boolean;
};
