"use client";

import { useState } from "react";
import { FileUpload } from "@/features/file-upload/ui/file-upload";
import { VideoProcessingStatus } from "@/features/video-processing/ui/video-processing-status";

type VideoUploadFlowProps = {
  context: string;
  contextId: string;
  disabled?: boolean;
  initialVideoAssetId?: string | null;
  onUploadSuccess?: (mediaAssetId: string) => void;
  onVideoAssetChange?: (mediaAssetId: string | null) => void;
};

export function VideoUploadFlow({
  context,
  contextId,
  disabled = false,
  initialVideoAssetId = null,
  onUploadSuccess,
  onVideoAssetChange,
}: VideoUploadFlowProps) {
  const [videoAssetId, setVideoAssetId] = useState<string | null>(initialVideoAssetId);

  if (videoAssetId) {
    return (
      <VideoProcessingStatus
        videoAssetId={videoAssetId}
        onUploadAnother={() => {
          setVideoAssetId(null);
          onVideoAssetChange?.(null);
        }}
      />
    );
  }

  return (
    <FileUpload
      assetType="video"
      context={context}
      contextId={contextId}
      acceptedTypes={["video/*"]}
      maxSizeBytes={5 * 1024 * 1024 * 1024}
      disabled={disabled}
      onSuccess={(asset) => {
        setVideoAssetId(asset.assetId);
        onVideoAssetChange?.(asset.assetId);
        onUploadSuccess?.(asset.assetId);
      }}
    />
  );
}
