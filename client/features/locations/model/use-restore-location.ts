import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRestoreLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: locationsApi.restoreLocation,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [locationsQueryOptions.baseKey] });
      toast.success("Локация восстановлена.");
    },
    onError: () => toast.error("Не удалось восстановить локацию."),
  });

  return {
    restoreLocation: mutation.mutateAsync,
    isPending: mutation.isPending,
  };
}
