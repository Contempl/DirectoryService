import {
  locationsApi,
  locationsQueryOptions,
  type BulkUpdateLocationsActivityRequest,
} from "@/entities/locations/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useBulkUpdateLocationsActivity() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (request: BulkUpdateLocationsActivityRequest) =>
      locationsApi.updateLocationsActivity(request),
    onSuccess: async (result) => {
      await queryClient.invalidateQueries({ queryKey: [locationsQueryOptions.baseKey] });

      if (result.errors.length > 0) {
        const failed = result.errors.map((error) => error.locationId).join(", ");
        toast.warning(
          `Обработано: ${result.processedCount}. Не обработано: ${result.errors.length}.`,
          { description: `ID с ошибками: ${failed}` },
        );
      } else {
        toast.success(`Успешно обработано локаций: ${result.processedCount}.`);
      }
    },
    onError: () => toast.error("Не удалось выполнить массовую операцию."),
  });

  return {
    updateLocationsActivity: mutation.mutateAsync,
    isPending: mutation.isPending,
  };
}
