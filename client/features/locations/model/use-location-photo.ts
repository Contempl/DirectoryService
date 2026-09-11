import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";

type SetPhotoVariables = {
  assetId: string;
  replace: boolean;
};

export function useLocationPhoto(locationId: string) {
  const queryClient = useQueryClient();
  const refreshLocations = () =>
    queryClient.invalidateQueries({ queryKey: [locationsQueryOptions.baseKey] });

  const setMutation = useMutation({
    mutationFn: ({ assetId, replace }: SetPhotoVariables) => {
      const request = { assetId };
      return replace
        ? locationsApi.replacePhoto(locationId, request)
        : locationsApi.attachPhoto(locationId, request);
    },
    onSuccess: refreshLocations,
  });

  const deleteMutation = useMutation({
    mutationFn: () => locationsApi.deletePhoto(locationId),
    onSuccess: refreshLocations,
  });

  return {
    setPhoto: setMutation.mutateAsync,
    resetSetPhoto: setMutation.reset,
    isSettingPhoto: setMutation.isPending,
    setPhotoError: setMutation.error,
    deletePhoto: deleteMutation.mutateAsync,
    isDeletingPhoto: deleteMutation.isPending,
    deletePhotoError: deleteMutation.error,
  };
}
