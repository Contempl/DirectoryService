import { positionsApi } from "@/entities/positions/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { positionsQueryOptions } from "@/entities/positions/api";

export function useDeletePosition() {
  const queryClient = useQueryClient();

  const { mutateAsync, isPending, isError, error } = useMutation({
    mutationFn: (id: string) => positionsApi.deletePosition(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [positionsQueryOptions.baseKey],
      });
    },
  });

  return {
    deletePosition: mutateAsync,
    isPending,
    isError,
    error,
  };
}