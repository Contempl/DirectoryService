import { positionsApi } from "@/entities/positions/api";
import { UpdatePositionRequest } from "@/entities/positions/types";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { positionsQueryOptions } from "@/entities/positions/api";

export function useUpdatePosition() {
  const queryClient = useQueryClient();

  const { mutateAsync, isPending, isError, error } = useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdatePositionRequest }) =>
      positionsApi.updatePosition(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [positionsQueryOptions.baseKey],
      });
    },
  });

  return {
    updatePosition: mutateAsync,
    isPending,
    isError,
    error,
  };
}