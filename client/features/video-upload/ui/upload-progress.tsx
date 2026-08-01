"use client";

type UploadProgressProps = {
  progress: number;
  uploadedChunks: number;
  totalChunks: number;
};

export function UploadProgress({ progress, uploadedChunks, totalChunks }: UploadProgressProps) {
  return (
    <div className="space-y-2">
      <div className="flex justify-between text-sm text-muted-foreground">
        <span>Uploading...</span>
        <span>{progress}%</span>
      </div>
      <div className="h-2 w-full rounded-full bg-secondary overflow-hidden">
        <div
          className="h-full rounded-full bg-primary transition-all duration-300"
          style={{ width: `${progress}%` }}
        />
      </div>
      <p className="text-xs text-muted-foreground text-center">
        Part {uploadedChunks} of {totalChunks}
      </p>
    </div>
  );
}
