import { departmentsApi, departmentsQueryOptions } from "@/entities/departments/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTreeStore } from "./use-tree-store";

type MoveDepartmentVariables = {
  departmentId: string;
  parentId: string | null;
};

export function useMoveDepartment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ departmentId, parentId }: MoveDepartmentVariables) =>
      departmentsApi.moveDepartment(departmentId, parentId),
    onSuccess: async () => {
      useTreeStore.getState().resetChildren();
      await queryClient.invalidateQueries({
        queryKey: [departmentsQueryOptions.baseKey],
      });
    },
  });
}
