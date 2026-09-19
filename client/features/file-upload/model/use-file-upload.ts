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
  partNumber?: number;
};

export type UploadStrategy = "auto" | "simple" | "multipart";

export const DEFAULT_MULTIPART_THRESHOLD_BYTES = 100 * 1024 * 1024;
export const DEFAULT_MULTIPART_CONCURRENCY = 3;
export const DEFAULT_PART_MAX_ATTEMPTS = 3;

export type PartRetryState = {
  partNumber: number;
  attempt: number;
  maxAttempts: number;
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
  strategy?: UploadStrategy;
  multipartThresholdBytes?: number;
  multipartConcurrency?: number;
  partMaxAttempts?: number;
  onStatusChange?: (status: UploadStatus) => void;
  onSuccess?: (asset: UploadedAsset) => void;
  onError?: (failure: UploadFailure) => void;
  onCancel?: () => void;
};

export function selectUploadStrategy(
  file: File,
  assetType: string,
  strategy: UploadStrategy = "auto",
  multipartThresholdBytes = DEFAULT_MULTIPART_THRESHOLD_BYTES
): Exclude<UploadStrategy, "auto"> {
  if (strategy !== "auto") return strategy;

  const isVideo = assetType.toLowerCase() === "video" || file.type.startsWith("video/");
  return isVideo || file.size >= multipartThresholdBytes ? "multipart" : "simple";
}

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
  const [partRetry, setPartRetry] = useState<PartRetryState | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);
  const uploadIdentityRef = useRef<{ mediaAssetId: string; uploadId: string } | null>(null);
  const cancellationRequestedRef = useRef(false);

  const changeStatus = useCallback((nextStatus: UploadStatus) => {
    setStatus(nextStatus);
    options.onStatusChange?.(nextStatus);
  }, [options]);

  const fail = useCallback((failure: UploadFailure) => {
    setPartRetry(null);
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
    setPartRetry(null);
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
    setPartRetry(null);
    setProgress(0);
    changeStatus("uploading");

    let failureKind: UploadErrorKind = "initiate";

    try {
      const strategy = selectUploadStrategy(
        file,
        options.assetType,
        options.strategy,
        options.multipartThresholdBytes
      );

      if (strategy === "simple") {
        failureKind = "storage";
        const assetId = await mediaApi.uploadFile(
          {
            file,
            assetType: options.assetType,
            context: options.context,
            contextId: options.contextId,
          },
          (uploadedBytes) => {
            const percentage = Math.round((uploadedBytes / file.size) * 100);
            setProgress(Math.min(percentage, 100));
          },
          controller.signal
        );

        const uploadedAsset: UploadedAsset = { assetId, status: "uploaded" };
        abortControllerRef.current = null;
        setAsset(uploadedAsset);
        setProgress(100);
        changeStatus("success");
        options.onSuccess?.(uploadedAsset);
        return;
      }

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
      const uploadedBytesByPart = new Map<number, number>();
      const requestedConcurrency = options.multipartConcurrency ?? DEFAULT_MULTIPART_CONCURRENCY;
      const normalizedConcurrency = Number.isFinite(requestedConcurrency)
        ? Math.max(1, Math.floor(requestedConcurrency))
        : DEFAULT_MULTIPART_CONCURRENCY;
      const concurrency = Math.max(
        1,
        Math.min(normalizedConcurrency, started.chunkUrls.length)
      );
      let nextChunkIndex = 0;
      let partUploadError: unknown = null;
      const requestedMaxAttempts = options.partMaxAttempts ?? DEFAULT_PART_MAX_ATTEMPTS;
      const maxAttempts = Number.isFinite(requestedMaxAttempts)
        ? Math.max(1, Math.floor(requestedMaxAttempts))
        : DEFAULT_PART_MAX_ATTEMPTS;

      const updateMultipartProgress = (partNumber: number, uploadedBytes: number, partSize: number) => {
        uploadedBytesByPart.set(partNumber, Math.min(uploadedBytes, partSize));
        const totalUploadedBytes = Array.from(uploadedBytesByPart.values())
          .reduce((total, value) => total + value, 0);
        setProgress(Math.min(Math.round((totalUploadedBytes / file.size) * 100), 100));
      };

      const uploadNextParts = async () => {
        while (!partUploadError && nextChunkIndex < started.chunkUrls.length) {
          const chunk = started.chunkUrls[nextChunkIndex];
          nextChunkIndex += 1;

          const start = (chunk.partNumber - 1) * started.chunkSize;
          const blob = file.slice(start, Math.min(start + started.chunkSize, file.size));

          let uploadedPart = false;
          let lastPartError: unknown = null;

          for (let attempt = 1; attempt <= maxAttempts && !controller.signal.aborted; attempt += 1) {
            if (attempt > 1) {
              setPartRetry({ partNumber: chunk.partNumber, attempt, maxAttempts });
              updateMultipartProgress(chunk.partNumber, 0, blob.size);
            }

            try {
              const eTag = await mediaApi.uploadPart(
                chunk.uploadUrl,
                blob,
                chunk.headers,
                (uploadedBytes) => updateMultipartProgress(chunk.partNumber, uploadedBytes, blob.size),
                controller.signal
              );
              partETags.push({ partNumber: chunk.partNumber, eTag });
              setPartRetry((current) => current?.partNumber === chunk.partNumber ? null : current);
              uploadedPart = true;
              break;
            } catch (caughtError) {
              lastPartError = caughtError;
            }
          }

          if (!uploadedPart) {
            const reason = lastPartError instanceof Error ? lastPartError.message : "Unknown storage error.";
            const error = new Error(
              `Part ${chunk.partNumber} failed after ${maxAttempts} attempt${maxAttempts === 1 ? "" : "s"}. ${reason}`
            );
            Object.assign(error, { partNumber: chunk.partNumber });

            if (!partUploadError) {
              partUploadError = error;
              controller.abort();
            }
          }
        }
      };

      await Promise.all(Array.from({ length: concurrency }, () => uploadNextParts()));

      if (partUploadError) throw partUploadError;
      setPartRetry(null);
      if (partETags.length !== started.chunkUrls.length) {
        throw new Error(
          `Uploaded ${partETags.length} of ${started.chunkUrls.length} required parts.`
        );
      }
      partETags.sort((left, right) => left.partNumber - right.partNumber);

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

      const failedPartNumber = caughtError instanceof Error
        ? (caughtError as Error & { partNumber?: number }).partNumber
        : undefined;
      fail({
        kind: failedPartNumber === undefined ? failureKind : "storage",
        message,
        partNumber: failedPartNumber,
      });
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
      setPartRetry(null);
      changeStatus("cancelled");
      options.onCancel?.();
    } catch (caughtError) {
      const message = caughtError instanceof Error ? caughtError.message : "Could not cancel upload.";
      cancellationRequestedRef.current = false;
      fail({ kind: "cancel", message });
    }
  }, [changeStatus, fail, options, status]);

  return { upload, cancel, reset, status, progress, error, asset, partRetry };
}
