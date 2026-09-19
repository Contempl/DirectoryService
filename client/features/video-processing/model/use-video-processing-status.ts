"use client";

import { useEffect, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { mediaApi } from "@/entities/media/api";
import type { VideoProcessingStatus } from "@/entities/media/types";

export type ProcessingConnectionStatus =
  | "idle"
  | "connecting"
  | "open"
  | "reconnecting"
  | "polling"
  | "closed";

export const MAX_CONSECUTIVE_SSE_ERRORS = 3;
export const PROCESSING_STATUS_POLL_INTERVAL_MS = 3_000;

export const videoProcessingStatusQueryKey = (videoAssetId: string) =>
  ["media", "video-processing-status", videoAssetId] as const;

function parseStatusEvent(event: MessageEvent<string>): VideoProcessingStatus | null {
  try {
    return JSON.parse(event.data) as VideoProcessingStatus;
  } catch {
    return null;
  }
}

export function useVideoProcessingStatus(videoAssetId: string | null) {
  const queryClient = useQueryClient();
  const [pollingVideoAssetId, setPollingVideoAssetId] = useState<string | null>(null);
  const [streamConnection, setStreamConnection] = useState<{
    videoAssetId: string;
    status: "open" | "reconnecting";
  } | null>(null);
  const isPolling = Boolean(videoAssetId && pollingVideoAssetId === videoAssetId);

  const statusQuery = useQuery({
    queryKey: videoProcessingStatusQueryKey(videoAssetId ?? "missing"),
    queryFn: () => mediaApi.getVideoProcessingStatus(videoAssetId!),
    enabled: Boolean(videoAssetId),
    retry: 2,
    staleTime: 0,
    refetchInterval: (query) =>
      isPolling && !query.state.data?.isTerminal
        ? PROCESSING_STATUS_POLL_INTERVAL_MS
        : false,
  });

  const status = statusQuery.data;
  const connectionStatus: ProcessingConnectionStatus = !videoAssetId
    ? "idle"
    : status?.isTerminal
      ? "closed"
      : isPolling
        ? "polling"
        : !statusQuery.isSuccess || streamConnection?.videoAssetId !== videoAssetId
          ? "connecting"
          : streamConnection.status;

  useEffect(() => {
    if (!videoAssetId || !statusQuery.isSuccess || status?.isTerminal || isPolling) {
      return;
    }

    let consecutiveErrors = 0;
    const eventSource = new EventSource(
      mediaApi.getVideoProcessingStatusStreamUrl(videoAssetId),
      { withCredentials: true }
    );

    const applyStatus = (event: MessageEvent<string>) => {
      const nextStatus = parseStatusEvent(event);
      if (!nextStatus) return;

      queryClient.setQueryData(
        videoProcessingStatusQueryKey(videoAssetId),
        nextStatus
      );
    };

    eventSource.onopen = () => {
      consecutiveErrors = 0;
      setStreamConnection({ videoAssetId, status: "open" });
    };
    eventSource.onerror = () => {
      consecutiveErrors += 1;

      if (consecutiveErrors >= MAX_CONSECUTIVE_SSE_ERRORS) {
        eventSource.close();
        setPollingVideoAssetId(videoAssetId);
        return;
      }

      setStreamConnection({ videoAssetId, status: "reconnecting" });
    };
    eventSource.addEventListener("initial", applyStatus as EventListener);
    eventSource.addEventListener("progress", applyStatus as EventListener);
    eventSource.addEventListener("final", applyStatus as EventListener);

    return () => {
      eventSource.close();
    };
  }, [isPolling, queryClient, status?.isTerminal, statusQuery.isSuccess, videoAssetId]);

  return {
    status,
    connectionStatus,
    isPolling,
    isLoading: statusQuery.isLoading,
    error: statusQuery.error,
    refetch: statusQuery.refetch,
  };
}
