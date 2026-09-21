"use client";

import {
  CheckCircle2,
  Clock3,
  LoaderCircle,
  Radio,
  RefreshCcw,
  XCircle,
} from "lucide-react";
import { Badge } from "@/shared/components/ui/badge";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { cn } from "@/shared/lib/utils";
import { VideoPlayback } from "@/features/video-player/ui/video-playback";
import { useVideoProcessingStatus } from "../model/use-video-processing-status";

type VideoProcessingStatusProps = {
  videoAssetId: string | null;
  className?: string;
  onUploadAnother?: () => void;
};

const stepLabels: Record<string, string> = {
  initialize: "Preparing processing",
  extract_metadata: "Reading video metadata",
  generate_hls: "Generating streaming files",
  upload_hls: "Uploading streaming files",
  generate_preview: "Generating preview",
  cleanup: "Cleaning temporary files",
};

function formatStep(step: string | null): string {
  if (!step) return "Processing video";
  return stepLabels[step] ?? step.replaceAll("_", " ");
}

export function VideoProcessingStatus({
  videoAssetId,
  className,
  onUploadAnother,
}: VideoProcessingStatusProps) {
  const processing = useVideoProcessingStatus(videoAssetId);

  if (!videoAssetId) return null;

  if (processing.isLoading) {
    return (
      <div className={cn("flex items-center gap-3 rounded-lg border p-4", className)}>
        <Spinner />
        <div>
          <p className="font-medium">Loading processing status</p>
          <p className="text-sm text-muted-foreground">Reading the latest state from File Service…</p>
        </div>
      </div>
    );
  }

  if (processing.error || !processing.status) {
    return (
      <div className={cn("space-y-3 rounded-lg border border-destructive/40 p-4", className)}>
        <div className="flex items-start gap-3">
          <XCircle className="mt-0.5 size-5 shrink-0 text-destructive" />
          <div>
            <p className="font-medium">Could not load processing status</p>
            <p className="text-sm text-destructive">
              {processing.error instanceof Error
                ? processing.error.message
                : "File Service did not return a status."}
            </p>
          </div>
        </div>
        <Button type="button" variant="outline" size="sm" onClick={() => void processing.refetch()}>
          <RefreshCcw /> Check again
        </Button>
      </div>
    );
  }

  const status = processing.status;
  const progress = Math.max(0, Math.min(status.progressPercentage, 100));
  const isQueued = status.status === "queued";
  const isProcessing = status.status === "processing";
  const isReady = status.status === "ready";
  const isFailed = status.status === "failed";
  const isDeleted = status.status === "deleted";
  const isUnavailable = isFailed || isDeleted;

  return (
    <div
      className={cn(
        "space-y-4 rounded-lg border p-4",
        isReady && "border-green-500/40",
        isUnavailable && "border-destructive/40",
        className
      )}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 items-start gap-3">
          {isQueued && <Clock3 className="mt-0.5 size-5 shrink-0 text-muted-foreground" />}
          {isProcessing && <LoaderCircle className="mt-0.5 size-5 shrink-0 animate-spin text-primary" />}
          {isReady && <CheckCircle2 className="mt-0.5 size-5 shrink-0 text-green-500" />}
          {isUnavailable && <XCircle className="mt-0.5 size-5 shrink-0 text-destructive" />}

          <div className="min-w-0">
            <p className="font-medium">
              {isQueued && "Video queued"}
              {isProcessing && formatStep(status.currentStep)}
              {isReady && "Video is ready"}
              {isFailed && "Video processing failed"}
              {isDeleted && "Video was deleted"}
              {!isQueued && !isProcessing && !isReady && !isUnavailable && "Video status"}
            </p>
            <p className="mt-1 break-all text-xs text-muted-foreground">{status.videoAssetId}</p>
          </div>
        </div>

        <Badge variant={isUnavailable ? "destructive" : isReady ? "default" : "secondary"}>
          {status.status}
        </Badge>
      </div>

      {(isQueued || isProcessing) && (
        <div className="space-y-2">
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">
              {isQueued ? "Waiting for a worker" : formatStep(status.currentStep)}
            </span>
            <span>{progress}%</span>
          </div>
          <div
            role="progressbar"
            aria-label="Video processing progress"
            aria-valuemin={0}
            aria-valuemax={100}
            aria-valuenow={progress}
            className="h-2 overflow-hidden rounded-full bg-secondary"
          >
            <div
              className="h-full rounded-full bg-primary transition-[width] duration-300"
              style={{ width: `${progress}%` }}
            />
          </div>
        </div>
      )}

      {isReady && <VideoPlayback videoAssetId={videoAssetId} />}

      {isUnavailable && (
        <div className="space-y-3">
          <p className="text-sm text-destructive">
            {status.errorMessage
              ?? (isDeleted
                ? "This video is no longer available."
                : "The processing pipeline did not provide an error message.")}
          </p>
          <Button type="button" variant="outline" size="sm" onClick={() => void processing.refetch()}>
            <RefreshCcw /> Check again
          </Button>
        </div>
      )}

      {status.isTerminal && onUploadAnother && (
        <Button type="button" variant="outline" size="sm" onClick={onUploadAnother}>
          Upload another video
        </Button>
      )}

      {!status.isTerminal && (
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <Radio className={cn("size-3.5", processing.connectionStatus === "open" && "text-green-500")} />
          {processing.connectionStatus === "open" && "Live updates connected"}
          {processing.connectionStatus === "connecting" && "Connecting to live updates…"}
          {processing.connectionStatus === "reconnecting" && "Live connection interrupted, reconnecting…"}
          {processing.connectionStatus === "polling" && "Using status polling fallback"}
        </div>
      )}
    </div>
  );
}
