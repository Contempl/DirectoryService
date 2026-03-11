import { apiClient } from "@/shared/api/axios-instance";
import { LocationDto } from "./types";
import { PagedResult } from "@/shared/api/types";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";


export type GetLocationsRequest = {
    department_ids?: string[], 
    search?: string,
    isActive: boolean,
    page: number,
    pageSize: number
}

export type CreateLocationDto = {
  name: string;
  city: string;
  street: string;
  house: string;
  apartment?: string | null;
  timezone: string;
}

export type CreateLocationRequest ={
  name: string;
  city: string;
  street: string;
  house: string;
  apartment?: string | null;
  timezone: string;
}


export type UpdateLocationRequest = {
  name: string;
  city: string;
  street: string;
  house: string;
  apartment?: string | null;
  timezone: string;
}


export const locationsApi = {
  getLocations: async (query?: GetLocationsRequest) => {
    const response = await apiClient.get<PagedResult<LocationDto>>("/locations", {
      params: query
        });
          return response.data;
  },

  createLocation: async (request: CreateLocationRequest) => {
    const response = await apiClient.post("/locations", request);
        
    return response.data;
  },

   updateLocation: async (id: string, request: UpdateLocationRequest) => {
    const response = await apiClient.put(`/locations/${id}`, request);
    return response.data;
  },

  deleteLocation: async (id: string) => {
    const response = await apiClient.delete(`/locations/${id}`);
    return response.data;
  },
}

export const locationsQueryOptions = {
  baseKey: "locations",

  getLocationsOptions: (query: GetLocationsRequest) =>
    queryOptions({
      queryKey: [locationsQueryOptions.baseKey, query],
      queryFn: () => locationsApi.getLocations(query),
      staleTime: 1000 * 60,
    }),

    getLocationInfiniteOptions: ({ pageSize, isActive }: { pageSize: number; isActive: boolean }) => {
      return infiniteQueryOptions({
        queryKey: [locationsQueryOptions.baseKey, { pageSize, isActive }],
        queryFn: ({ pageParam }) => locationsApi.getLocations({ page: pageParam ?? 1, pageSize, isActive }),
        initialPageParam: 1,
        getNextPageParam: (lastPage) => {
          if (!lastPage.items || lastPage.page >= lastPage.totalPages) {
            return undefined;
          }
          return lastPage.page + 1;
        },
        select: (data): PagedResult<LocationDto> => ({
          items: data.pages.flatMap((page) => page?.items ?? []),
          totalCount: data.pages[0]?.totalCount ?? 0,
          page: data.pages[0]?.page ?? 1,
          pageSize: data.pages[0]?.pageSize ?? pageSize,
          totalPages: data.pages[0]?.totalPages ?? 0
        })
      });
    },
};