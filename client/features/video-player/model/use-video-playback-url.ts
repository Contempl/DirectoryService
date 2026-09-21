import { useQuery } from "@tanstack/react-query";
import { mediaApi } from "@/entities/media/api";

export const videoPlaybackUrlQueryKey = (videoAssetId: string) =>
  ["media", "video-playback-url", videoAssetId] as const;

export function useVideoPlaybackUrl(videoAssetId: string) {
  return useQuery({
    queryKey: videoPlaybackUrlQueryKey(videoAssetId),
    queryFn: ({ signal }) => mediaApi.getDownloadUrl(videoAssetId, signal),
    retry: 1,
    staleTime: 0,
    gcTime: 0,
  });
}
