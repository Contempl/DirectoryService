"use client";

import { useCallback, useState } from "react";
import { RefreshCcw, XCircle } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { cn } from "@/shared/lib/utils";
import { useVideoPlaybackUrl } from "../model/use-video-playback-url";
import { HlsVideoPlayer } from "./hls-video-player";

type VideoPlaybackProps = {
  videoAssetId: string;
  className?: string;
};

export function VideoPlayback({ videoAssetId, className }: VideoPlaybackProps) {
  const playbackUrl = useVideoPlaybackUrl(videoAssetId);
  const [playerAttempt, setPlayerAttempt] = useState(0);
  const [playerError, setPlayerError] = useState<string | null>(null);
  const [isPlayerReady, setIsPlayerReady] = useState(false);

  const handleReady = useCallback(() => {
    setPlayerError(null);
    setIsPlayerReady(true);
  }, []);

  const handleError = useCallback((message: string) => {
    setPlayerError(message);
    setIsPlayerReady(false);
  }, []);

  const handleRetry = async () => {
    setPlayerError(null);
    setIsPlayerReady(false);

    const result = await playbackUrl.refetch();
    if (result.isSuccess) {
      // Remount even when File Service returns the same URL.
      setPlayerAttempt((attempt) => attempt + 1);
    }
  };

  if (playbackUrl.isPending) {
    return (
      <div className={cn("flex items-center gap-3 rounded-lg border p-4", className)}>
        <Spinner />
        <div>
          <p className="font-medium">Loading video</p>
          <p className="text-sm text-muted-foreground">Requesting a playback URL from File Service...</p>
        </div>
      </div>
    );
  }

  const requestError = playbackUrl.error;
  if (requestError || !playbackUrl.data) {
    return (
      <PlaybackError
        className={className}
        message={requestError instanceof Error ? requestError.message : "File Service returned no playback URL."}
        isRetrying={playbackUrl.isFetching}
        onRetry={() => void handleRetry()}
      />
    );
  }

  return (
    <div className={cn("relative overflow-hidden rounded-lg", className)}>
      {!isPlayerReady && !playerError && (
        <div className="absolute inset-0 z-10 flex items-center justify-center gap-3 bg-black text-white">
          <Spinner />
          <span className="text-sm">Loading video stream...</span>
        </div>
      )}

      {playerError ? (
        <PlaybackError
          message={playerError}
          isRetrying={playbackUrl.isFetching}
          onRetry={() => void handleRetry()}
        />
      ) : (
        <HlsVideoPlayer
          key={`${playbackUrl.data}-${playerAttempt}`}
          sourceUrl={playbackUrl.data}
          onReady={handleReady}
          onError={handleError}
        />
      )}
    </div>
  );
}

type PlaybackErrorProps = {
  message: string;
  isRetrying: boolean;
  onRetry: () => void;
  className?: string;
};

function PlaybackError({ message, isRetrying, onRetry, className }: PlaybackErrorProps) {
  return (
    <div className={cn("space-y-3 rounded-lg border border-destructive/40 p-4", className)}>
      <div className="flex items-start gap-3">
        <XCircle className="mt-0.5 size-5 shrink-0 text-destructive" />
        <div>
          <p className="font-medium">Could not play video</p>
          <p className="text-sm text-destructive">{message}</p>
        </div>
      </div>
      <Button type="button" variant="outline" size="sm" disabled={isRetrying} onClick={onRetry}>
        {isRetrying ? <Spinner /> : <RefreshCcw />}
        {isRetrying ? "Refreshing..." : "Refresh playback URL"}
      </Button>
    </div>
  );
}
