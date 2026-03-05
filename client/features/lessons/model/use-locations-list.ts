import { locationsQueryOptions } from "@/entities/locations/api";
import { useQuery } from "@tanstack/react-query";

export function useLocationsList(
  { page, pageSize, isActive }: 
  { page: number; pageSize: number; isActive: boolean }
) {
  const query = { page, pageSize, isActive };

  const queryResult = useQuery(
    locationsQueryOptions.getLocationsOptions(query)
  );

  return {
    ...queryResult,
    totalCount: queryResult.data?.totalCount ?? 0,
  };
}