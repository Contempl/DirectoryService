import { departmentsApi } from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useDepartmentRoots(page = 1, size = 20, prefetch = 3) {
  const { data, isPending, isError, error } = useQuery({
    queryKey: ["departments", "roots", { page, size, prefetch }],
    queryFn: () => departmentsApi.getRoots({ page, size, prefetch }),
  });

  return {
    data,
    isPending,
    isError,
    error,
  };
}