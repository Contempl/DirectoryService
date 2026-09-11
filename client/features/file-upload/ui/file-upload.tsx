"use client";

import { DragEvent, useRef, useState } from "react";
import { CheckCircle, FileUp, RotateCcw, Upload, X, XCircle } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { cn } from "@/shared/lib/utils";
import {
  UploadedAsset,
  UploadFailure,
  UploadStatus,
  useFileUpload,
} from "../model/use-file-upload";

export type FileUploadProps = {
  assetType: string;
  context: string;
  contextId: string;
  acceptedTypes?: string[];
  maxSizeBytes?: number;
  displayMode?: "dropzone" | "compact";
  disabled?: boolean;
  onStatusChange?: (status: UploadStatus) => void;
  onSuccess?: (asset: UploadedAsset) => void;
  onError?: (failure: UploadFailure) => void;
  onCancel?: () => void;
};

function formatSize(bytes?: number): string | null {
  if (bytes === undefined) return null;
  return `${Math.ceil(bytes / 1024 / 1024)} MB`;
}

export function FileUpload({
  acceptedTypes = [],
  maxSizeBytes,
  displayMode = "dropzone",
  disabled = false,
  ...options
}: FileUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [fileName, setFileName] = useState<string | null>(null);
  const uploader = useFileUpload({
    ...options,
    acceptedTypes,
    maxSizeBytes,
  });
  const isActive = uploader.status === "uploading" || uploader.status === "completing";
  const accept = acceptedTypes.join(",");

  const selectFile = (file?: File) => {
    if (!file || disabled || isActive) return;
    setFileName(file.name);
    void uploader.upload(file);
  };

  const reset = () => {
    uploader.reset();
    setFileName(null);
    if (inputRef.current) inputRef.current.value = "";
  };

  const handleDrop = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setIsDragging(false);
    selectFile(event.dataTransfer.files[0]);
  };

  const picker = (
    <input
      ref={inputRef}
      type="file"
      accept={accept || undefined}
      disabled={disabled || isActive}
      className="hidden"
      onChange={(event) => selectFile(event.target.files?.[0])}
    />
  );

  if (displayMode === "compact" && uploader.status === "idle") {
    return (
      <>
        <Button type="button" variant="outline" disabled={disabled} onClick={() => inputRef.current?.click()}>
          <Upload className="mr-2 h-4 w-4" />
          Choose file
        </Button>
        {picker}
      </>
    );
  }

  return (
    <div className="space-y-4">
      {uploader.status === "idle" && (
        <div
          role="button"
          tabIndex={disabled ? -1 : 0}
          className={cn(
            "flex min-h-48 flex-col items-center justify-center gap-3 rounded-lg border-2 border-dashed p-8 text-center transition-colors",
            disabled ? "cursor-not-allowed opacity-50" : "cursor-pointer hover:border-primary/60",
            isDragging && "border-primary bg-primary/5",
          )}
          onClick={() => !disabled && inputRef.current?.click()}
          onKeyDown={(event) => {
            if ((event.key === "Enter" || event.key === " ") && !disabled) inputRef.current?.click();
          }}
          onDragEnter={(event) => {
            event.preventDefault();
            if (!disabled) setIsDragging(true);
          }}
          onDragOver={(event) => event.preventDefault()}
          onDragLeave={() => setIsDragging(false)}
          onDrop={handleDrop}
        >
          <FileUp className="h-9 w-9 text-muted-foreground" />
          <div>
            <p className="font-medium">Drop a file here or click to browse</p>
            <p className="mt-1 text-xs text-muted-foreground">
              {acceptedTypes.length ? acceptedTypes.join(", ") : "Any supported file"}
              {formatSize(maxSizeBytes) ? ` · up to ${formatSize(maxSizeBytes)}` : ""}
            </p>
          </div>
          {picker}
        </div>
      )}

      {(uploader.status === "uploading" || uploader.status === "completing") && (
        <div className="space-y-3 rounded-lg border p-4">
          <div className="flex items-center justify-between gap-3 text-sm">
            <span className="truncate">{fileName}</span>
            <span className="text-muted-foreground">
              {uploader.status === "completing" ? "Completing…" : `${uploader.progress}%`}
            </span>
          </div>
          <div className="h-2 overflow-hidden rounded-full bg-secondary">
            <div
              className="h-full rounded-full bg-primary transition-[width] duration-300"
              style={{ width: `${uploader.progress}%` }}
            />
          </div>
          <Button type="button" variant="outline" size="sm" onClick={() => void uploader.cancel()}>
            <X className="mr-2 h-4 w-4" />
            Cancel
          </Button>
        </div>
      )}

      {uploader.status === "success" && (
        <div className="flex items-start gap-3 rounded-lg border border-green-500/40 p-4">
          <CheckCircle className="mt-0.5 h-5 w-5 shrink-0 text-green-500" />
          <div className="min-w-0 flex-1">
            <p className="font-medium">Upload complete</p>
            <p className="truncate text-sm text-muted-foreground">{fileName}</p>
            <p className="break-all text-xs text-muted-foreground">{uploader.asset?.assetId}</p>
          </div>
          <Button type="button" variant="ghost" size="sm" onClick={reset}>Upload another</Button>
        </div>
      )}

      {uploader.status === "cancelled" && (
        <div className="flex items-center justify-between rounded-lg border p-4">
          <div>
            <p className="font-medium">Upload cancelled</p>
            <p className="text-sm text-muted-foreground">No file was attached.</p>
          </div>
          <Button type="button" variant="outline" size="sm" onClick={reset}>
            <RotateCcw className="mr-2 h-4 w-4" /> Retry
          </Button>
        </div>
      )}

      {uploader.status === "error" && (
        <div className="flex items-start gap-3 rounded-lg border border-destructive/40 p-4">
          <XCircle className="mt-0.5 h-5 w-5 shrink-0 text-destructive" />
          <div className="min-w-0 flex-1">
            <p className="font-medium">{uploader.error?.kind === "validation" ? "File is not valid" : "Upload failed"}</p>
            <p className="text-sm text-destructive">{uploader.error?.message}</p>
          </div>
          <Button type="button" variant="outline" size="sm" onClick={reset}>Try again</Button>
        </div>
      )}
    </div>
  );
}
