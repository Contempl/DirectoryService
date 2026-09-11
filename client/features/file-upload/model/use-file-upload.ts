"use client";

import { useCallback, useRef, useState } from "react";
import { mediaApi } from "@/entities/media/api";

export type UploadStatus =
  | "idle"
  | "uploading"
  | "completing"
  | "success"
  | "error"
  | "cancelled";

export type UploadErrorKind = "validation" | "initiate" | "storage" | "complete" | "cancel";

export type UploadFailure = {
  kind: UploadErrorKind;
  message: string;
};

export type UploadedAsset = {
  assetId: string;
  status: "uploaded";
};

type UseFileUploadOptions = {
  assetType: string;
  context: string;
  contextId: string;
  acceptedTypes?: string[];
  maxSizeBytes?: number;
  onStatusChange?: (status: UploadStatus) => void;
  onSuccess?: (asset: UploadedAsset) => void;
  onError?: (failure: UploadFailure) => void;
  onCancel?: () => void;
};

function acceptsFile(file: File, acceptedTypes: string[]): boolean {
  return acceptedTypes.some((acceptedType) => {
    if (acceptedType.endsWith("/*")) {
      return file.type.startsWith(acceptedType.slice(0, -1));
    }

    return file.type === acceptedType;
  });
}

export function useFileUpload(options: UseFileUploadOptions) {
  const [status, setStatus] = useState<UploadStatus>("idle");
  const [progress, setProgress] = useState(0);
  const [error, setError] = useState<UploadFailure | null>(null);
  const [asset, setAsset] = useState<UploadedAsset | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);
  const uploadIdentityRef = useRef<{ mediaAssetId: string; uploadId: string } | null>(null);
  const cancellationRequestedRef = useRef(false);

  const changeStatus = useCallback((nextStatus: UploadStatus) => {
    setStatus(nextStatus);
    options.onStatusChange?.(nextStatus);
  }, [options]);

  const fail = useCallback((failure: UploadFailure) => {
    setError(failure);
    changeStatus("error");
    options.onError?.(failure);
  }, [changeStatus, options]);

  const reset = useCallback(() => {
    abortControllerRef.current?.abort();
    abortControllerRef.current = null;
    uploadIdentityRef.current = null;
    cancellationRequestedRef.current = false;
    setProgress(0);
    setError(null);
    setAsset(null);
    changeStatus("idle");
  }, [changeStatus]);

  const validate = useCallback((file: File): UploadFailure | null => {
    if (file.size === 0) {
      return { kind: "validation", message: "The selected file is empty." };
    }

    if (options.maxSizeBytes !== undefined && file.size > options.maxSizeBytes) {
      return {
        kind: "validation",
        message: `The file is larger than the allowed ${Math.ceil(options.maxSizeBytes / 1024 / 1024)} MB.`,
      };
    }

    if (options.acceptedTypes?.length && !acceptsFile(file, options.acceptedTypes)) {
      return {
        kind: "validation",
        message: `Unsupported file type: ${file.type || "unknown"}.`,
      };
    }

    return null;
  }, [options.acceptedTypes, options.maxSizeBytes]);

  const upload = useCallback(async (file: File) => {
    if (status === "uploading" || status === "completing") return;

    const validationFailure = validate(file);
    if (validationFailure) {
      fail(validationFailure);
      return;
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;
    cancellationRequestedRef.current = false;
    setError(null);
    setAsset(null);
    setProgress(0);
    changeStatus("uploading");

    let failureKind: UploadErrorKind = "initiate";

    try {
      const started = await mediaApi.startMultipartUpload({
        fileName: file.name,
        assetType: options.assetType,
        contentType: file.type || "application/octet-stream",
        size: file.size,
        context: options.context,
        contextId: options.contextId,
      }, controller.signal);

      uploadIdentityRef.current = {
        mediaAssetId: started.mediaAssetId,
        uploadId: started.uploadId,
      };

      failureKind = "storage";
      const partETags: Array<{ partNumber: number; eTag: string }> = [];

      for (let index = 0; index < started.chunkUrls.length; index += 1) {
        const chunk = started.chunkUrls[index];
        const start = (chunk.partNumber - 1) * started.chunkSize;
        const blob = file.slice(start, Math.min(start + started.chunkSize, file.size));
        const response = await fetch(chunk.uploadUrl, {
          method: "PUT",
          headers: chunk.headers,
          body: blob,
          signal: controller.signal,
        });

        if (!response.ok) {
          throw new Error(`Storage rejected part ${chunk.partNumber} with status ${response.status}.`);
        }

        const eTag = response.headers.get("ETag")?.replace(/"/g, "");
        if (!eTag) throw new Error(`Storage did not return an ETag for part ${chunk.partNumber}.`);

        partETags.push({ partNumber: chunk.partNumber, eTag });
        setProgress(Math.round(((index + 1) / started.chunkUrls.length) * 100));
      }

      failureKind = "complete";
      changeStatus("completing");
      const completed = await mediaApi.completeMultipartUpload({
        mediaAssetId: started.mediaAssetId,
        uploadId: started.uploadId,
        partETags,
      }, controller.signal);

      const uploadedAsset: UploadedAsset = {
        assetId: completed.mediaAssetId,
        status: "uploaded",
      };
      uploadIdentityRef.current = null;
      abortControllerRef.current = null;
      setAsset(uploadedAsset);
      setProgress(100);
      changeStatus("success");
      options.onSuccess?.(uploadedAsset);
    } catch (caughtError) {
      if (cancellationRequestedRef.current) return;

      const message = caughtError instanceof Error ? caughtError.message : "Upload failed.";
      const identity = uploadIdentityRef.current;
      if (identity) {
        try {
          await mediaApi.cancelMultipartUpload(identity);
        } catch (cancelError) {
          const cancelMessage = cancelError instanceof Error ? cancelError.message : "Cleanup failed.";
          fail({ kind: "cancel", message: `${message} ${cancelMessage}` });
          return;
        }
      }

      fail({ kind: failureKind, message });
    } finally {
      abortControllerRef.current = null;
    }
  }, [changeStatus, fail, options, status, validate]);

  const cancel = useCallback(async () => {
    if (status !== "uploading" && status !== "completing") return;

    cancellationRequestedRef.current = true;
    abortControllerRef.current?.abort();
    const identity = uploadIdentityRef.current;

    try {
      if (identity) await mediaApi.cancelMultipartUpload(identity);
      uploadIdentityRef.current = null;
      setProgress(0);
      setError(null);
      changeStatus("cancelled");
      options.onCancel?.();
    } catch (caughtError) {
      const message = caughtError instanceof Error ? caughtError.message : "Could not cancel upload.";
      cancellationRequestedRef.current = false;
      fail({ kind: "cancel", message });
    }
  }, [changeStatus, fail, options, status]);

  return { upload, cancel, reset, status, progress, error, asset };
}
