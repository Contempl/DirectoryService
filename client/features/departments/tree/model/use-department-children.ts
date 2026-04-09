import { departmentsApi } from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useDepartmentChildren(
  parentId: string,
  page = 1,
  size = 20,
  enabled = false
) {
  const query = useQuery({
    queryKey: ["departments", "children", parentId, { page, size }],
    queryFn: () => departmentsApi.getChildren(parentId, { page, size }),
    enabled,
    placeholderData: (previousData) => previousData, 
  });

  return {
    data: query.data,
    isPending: query.isPending,
    isLoading: query.isLoading,   
    isFetching: query.isFetching,
    isError: query.isError,
    error: query.error,
  };
}