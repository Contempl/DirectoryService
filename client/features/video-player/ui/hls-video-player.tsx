"use client";

import Hls from "hls.js";
import { useEffect, useRef } from "react";
import { cn } from "@/shared/lib/utils";

type HlsVideoPlayerProps = {
  sourceUrl: string;
  className?: string;
  onReady?: () => void;
  onError?: (message: string) => void;
};

const HLS_CONTENT_TYPE = "application/vnd.apple.mpegurl";

export function HlsVideoPlayer({
  sourceUrl,
  className,
  onReady,
  onError,
}: HlsVideoPlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    const video = videoRef.current;
    if (!video) return;

    if (video.canPlayType(HLS_CONTENT_TYPE)) {
      const handleLoadedMetadata = () => onReady?.();
      const handleError = () => onError?.("The browser could not load the video stream.");

      video.addEventListener("loadedmetadata", handleLoadedMetadata);
      video.addEventListener("error", handleError);
      video.src = sourceUrl;

      return () => {
        video.removeEventListener("loadedmetadata", handleLoadedMetadata);
        video.removeEventListener("error", handleError);
        video.removeAttribute("src");
        video.load();
      };
    }

    if (!Hls.isSupported()) {
      onError?.("This browser does not support HLS video playback.");
      return;
    }

    const hls = new Hls();

    hls.on(Hls.Events.MANIFEST_PARSED, () => onReady?.());
    hls.on(Hls.Events.ERROR, (_, data) => {
      if (!data.fatal) return;

      onError?.(`The player could not load the video stream (${data.details}).`);
    });

    hls.loadSource(sourceUrl);
    hls.attachMedia(video);

    return () => hls.destroy();
  }, [sourceUrl, onReady, onError]);

  return (
    <video
      ref={videoRef}
      controls
      playsInline
      preload="metadata"
      className={cn("aspect-video w-full rounded-lg bg-black", className)}
    />
  );
}
