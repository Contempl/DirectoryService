import { mediaApi } from "@/entities/media/api";
import { useUploadStore } from "./upload-store";

type UseVideoUploadOptions = {
  context: string;
  contextId: string;
  onSuccess?: (mediaAssetId: string) => void;
  onError?: (error: Error) => void;
};

export function useVideoUpload({ context, contextId, onSuccess, onError }: UseVideoUploadOptions) {
  const store = useUploadStore();

  const upload = async (file: File) => {
    if (!file.type.startsWith("video/")) {
      const err = new Error("Only video files are allowed");
      store._setError(err.message);
      onError?.(err);
      return;
    }

    let mediaAssetId: string | undefined;
    let uploadId: string | undefined;

    try {
      const response = await mediaApi.startMultipartUpload({
        fileName: file.name,
        assetType: "video",
        contentType: file.type,
        size: file.size,
        context,
        contextId,
      });

      mediaAssetId = response.mediaAssetId;
      uploadId = response.uploadId;

      store._setUploading(response.chunkUrls.length);

      const partETags: { partNumber: number; eTag: string }[] = [];

      for (const chunk of response.chunkUrls) {
        const start = (chunk.partNumber - 1) * response.chunkSize;
        const end = Math.min(start + response.chunkSize, file.size);
        const blob = file.slice(start, end);

        const s3Response = await fetch(chunk.uploadUrl, {
          method: "PUT",
          body: blob,
        });

        if (!s3Response.ok) {
          throw new Error(`Chunk ${chunk.partNumber} upload failed: ${s3Response.status}`);
        }

        const etag = s3Response.headers.get("ETag")?.replace(/"/g, "") ?? "";
        partETags.push({ partNumber: chunk.partNumber, eTag: etag });

        store._setChunkUploaded(chunk.partNumber);
      }

      await mediaApi.completeMultipartUpload({ mediaAssetId, uploadId, partETags });

      store._setSuccess(mediaAssetId);
      onSuccess?.(mediaAssetId);
    } catch (error) {
      const err = error instanceof Error ? error : new Error("Upload failed");
      store._setError(err.message);

      if (mediaAssetId && uploadId) {
        await mediaApi.cancelMultipartUpload({ mediaAssetId, uploadId }).catch(() => {});
      }

      onError?.(err);
    }
  };

  const progress =
    store.totalChunks > 0 ? Math.round((store.uploadedChunks / store.totalChunks) * 100) : 0;

  return {
    upload,
    status: store.status,
    progress,
    uploadedChunks: store.uploadedChunks,
    totalChunks: store.totalChunks,
    error: store.error,
    mediaAssetId: store.mediaAssetId,
    reset: store.reset,
  };
}
