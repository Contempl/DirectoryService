import { locationsApi, locationsQueryOptions, UpdateLocationRequest } from "@/entities/locations/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export const useUpdateLocation = () => {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateLocationRequest }) =>
      locationsApi.updateLocation(id, data),
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      }),
    onError: () => {
      toast.error("Failed to update location.");
    },
    onSuccess: () => {
      toast.success("Location updated successfully.");
    },
  });

  return {
    updateLocation: mutation.mutateAsync,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  }
}