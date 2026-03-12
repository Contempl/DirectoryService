import { locationsQueryOptions } from "@/entities/locations/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useCallback } from "react";

export function useLocationsList(search: string | undefined, pageSize: number, isActive: boolean) {
  const {
    data,
    isPending,
    error,
    isError,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
    refetch,
  } = useInfiniteQuery({
    ...locationsQueryOptions.getLocationInfiniteOptions({ search, pageSize, isActive }),
  });


  const cursorRef : React.RefCallback<HTMLDivElement> = useCallback(
    (el) => {
      const observer = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting) {
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
    error,
    isError,
    isFetchingNextPage,
    refetch,
    cursorRef
  };
}