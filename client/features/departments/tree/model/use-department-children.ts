import { departmentsApi } from "@/entities/departments/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useMemo } from "react";

export function useDepartmentChildren(
  parentId: string,
  size = 20,
  enabled = false
) {
  const query = useInfiniteQuery({
    queryKey: ["departments", "children", parentId, { size }],
    queryFn: ({ pageParam }) =>
      departmentsApi.getChildren(parentId, { page: pageParam, size }),
    initialPageParam: 1,
    getNextPageParam: (lastPage, pages) =>
      lastPage.length === size ? pages.length + 1 : undefined,
    enabled,
    staleTime: Infinity,
  });

  const data = useMemo(() => query.data?.pages.flat(), [query.data]);

  return {
    data,
    isPending: query.isPending,
    isFetching: query.isFetching,
    isFetchingNextPage: query.isFetchingNextPage,
    hasNextPage: query.hasNextPage,
    fetchNextPage: query.fetchNextPage,
    isError: query.isError,
    error: query.error,
  };
}
