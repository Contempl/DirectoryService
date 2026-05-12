"use client";

import { useRef } from "react";
import { CheckCircle, Upload, XCircle } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import { useVideoUpload } from "../model/use-video-upload";
import { UploadProgress } from "./upload-progress";

type VideoUploadDialogProps = {
  context: string;
  contextId: string;
  trigger: React.ReactNode;
  onSuccess?: (mediaAssetId: string) => void;
};

export function VideoUploadDialog({
  context,
  contextId,
  trigger,
  onSuccess,
}: VideoUploadDialogProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const { upload, status, progress, uploadedChunks, totalChunks, error, reset } = useVideoUpload({
    context,
    contextId,
    onSuccess,
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) upload(file);
  };

  const handleOpenChange = (open: boolean) => {
    if (!open) {
      reset();
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  };

  return (
    <Dialog onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>{trigger}</DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Upload Video</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {status === "idle" && (
            <div
              className="flex flex-col items-center justify-center gap-3 rounded-lg border-2 border-dashed border-muted-foreground/30 p-10 cursor-pointer hover:border-muted-foreground/60 transition-colors"
              onClick={() => fileInputRef.current?.click()}
            >
              <Upload className="h-8 w-8 text-muted-foreground" />
              <div className="text-center">
                <p className="text-sm font-medium">Click to select a video</p>
                <p className="text-xs text-muted-foreground mt-1">MP4, MOV, AVI and other video formats</p>
              </div>
              <input
                ref={fileInputRef}
                type="file"
                accept="video/*"
                className="hidden"
                onChange={handleFileChange}
              />
            </div>
          )}

          {status === "uploading" && (
            <UploadProgress
              progress={progress}
              uploadedChunks={uploadedChunks}
              totalChunks={totalChunks}
            />
          )}

          {status === "success" && (
            <div className="flex flex-col items-center gap-2 py-6">
              <CheckCircle className="h-10 w-10 text-green-500" />
              <p className="text-sm font-medium">Upload complete</p>
            </div>
          )}

          {status === "error" && (
            <div className="flex flex-col items-center gap-3 py-4">
              <XCircle className="h-10 w-10 text-destructive" />
              <p className="text-sm text-destructive text-center">{error}</p>
              <Button variant="outline" size="sm" onClick={reset}>
                Try again
              </Button>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
