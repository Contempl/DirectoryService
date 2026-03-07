import { useMutation, useQueryClient  } from "@tanstack/react-query"
import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { toast } from "sonner";

export function useDeleteLocation() {
  const queryClient = useQueryClient();

  const mutation =  useMutation({
    mutationFn: locationsApi.deleteLocation,
    onSettled: () => 
      queryClient.invalidateQueries({
         queryKey: [locationsQueryOptions.baseKey] 
        }),
      onError: () => {
        toast.error("Failed to delete location.");
      },
      onSuccess: () => {
        toast.success("Location deleted successfully.");
      },
  });

  return {
    deleteLocation: mutation.mutateAsync,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  }
}