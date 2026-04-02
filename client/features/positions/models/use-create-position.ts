import { positionsApi } from "@/entities/positions/api";
import { CreatePositionRequest } from "@/entities/positions/types";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { positionsQueryOptions } from "@/entities/positions/api";

export function useCreatePosition() {
  const queryClient = useQueryClient();

  const { mutateAsync, isPending, isError, error } = useMutation({
    mutationFn: (request: CreatePositionRequest) =>
      positionsApi.createPosition(request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [positionsQueryOptions.baseKey],
      });
    },
  });

  return {
    createPosition: mutateAsync,
    isPending,
    isError,
    error,
  };
}