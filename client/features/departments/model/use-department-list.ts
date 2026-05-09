import { departmentsQueryOptions } from "@/entities/departments/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useCallback } from "react";
import { DepartmentListId } from "./department-list-store";

export function useDepartmentsList(
  search: string,
  pageSize: number,
  stateId?: DepartmentListId
) {
  const {
    data,
    isPending,
    isError,
    error,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
  } = useInfiniteQuery({
    ...departmentsQueryOptions.getDepartmentsInfiniteOptions({
      search,
      pageSize,
      page: 1,
    }),
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