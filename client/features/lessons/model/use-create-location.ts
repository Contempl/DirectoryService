import { useMutation, useQueryClient  } from "@tanstack/react-query"
import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { toast } from "sonner";

export function useCreateLocation() {
  const queryClient = useQueryClient();

  const mutation =  useMutation({
    mutationFn: locationsApi.createLocation,
    onSettled: () => 
      queryClient.invalidateQueries({
         queryKey: [locationsQueryOptions.baseKey] 
        }),
      onError: () => {
        toast.error("Failed to create location.");
      },
      onSuccess: () => {
        toast.success("Location created successfully.");
      },
  });

  return {
    createLocation: mutation.mutateAsync,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  }
}