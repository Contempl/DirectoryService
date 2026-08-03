import { departmentsApi } from "@/entities/departments/api";
import { useInfiniteQuery } from "@tanstack/react-query";

export function useDepartmentRoots(size = 20) {
  const query = useInfiniteQuery({
    queryKey: ["departments", "tree", { size }],
    queryFn: ({ pageParam }) =>
      departmentsApi.getRoots({ page: pageParam, size }),
    initialPageParam: 1,
    getNextPageParam: (lastPage, pages) =>
      lastPage.length === size ? pages.length + 1 : undefined,
  });

  return {
    data: query.data?.pages.flat(),
    isPending: query.isPending,
    isError: query.isError,
    error: query.error,
    hasNextPage: query.hasNextPage,
    isFetchingNextPage: query.isFetchingNextPage,
    fetchNextPage: query.fetchNextPage,
  };
}
