"use client";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import { VideoUploadFlow } from "./video-upload-flow";

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

        <VideoUploadFlow
          context={context}
          contextId={contextId}
          onUploadSuccess={onSuccess}
        />
      </DialogContent>
    </Dialog>
  );
}
