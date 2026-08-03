import { positionsQueryOptions } from "@/entities/positions/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useCallback } from "react";

export function usePositionsList(
  search: string | undefined,
  pageSize: number,
  isActive: boolean,
  departmentIds: string[],
  enabled = true
) {
  const {
    data,
    isPending,
    error,
    isError,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
  } = useInfiniteQuery({
    ...positionsQueryOptions.getPositionsInfiniteOptions({
      search,
      pageSize,
      isActive,
      departmentIds,
      page: 1,
    }),
    enabled,
  });

  const cursorRef: React.RefCallback<HTMLDivElement> = useCallback(
    (el) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
            fetchNextPage();
          }
        },
        { threshold: 0.5 }
      );

      if (el) {
        observer.observe(el);
        return () => observer.disconnect();
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage]
  );

  return {
    data,
    isPending,
    isError,
    error,
    isFetchingNextPage,
    cursorRef,
  };
}
