"use client";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import { FileUpload } from "@/features/file-upload/ui/file-upload";

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
  return (
    <Dialog>
      <DialogTrigger asChild>{trigger}</DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Upload Video</DialogTitle>
        </DialogHeader>

        <FileUpload
          assetType="video"
          context={context}
          contextId={contextId}
          acceptedTypes={["video/*"]}
          maxSizeBytes={5 * 1024 * 1024 * 1024}
          onSuccess={(asset) => onSuccess?.(asset.assetId)}
        />
      </DialogContent>
    </Dialog>
  );
}
