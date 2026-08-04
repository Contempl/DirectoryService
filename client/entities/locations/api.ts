import { apiClient } from "@/shared/api/axios-instance";
import { LocationDto } from "./types";
import { PagedResult } from "@/shared/api/types";
import { infiniteQueryOptions } from "@tanstack/react-query";


export type GetLocationsRequest = {
    departmentIds?: string[],
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
    const params = new URLSearchParams();
    if (query?.search) params.set("search", query.search);
    if (query) {
      params.set("isActive", String(query.isActive));
      params.set("page", String(query.page));
      params.set("pageSize", String(query.pageSize));
      query.departmentIds?.forEach((id) => params.append("departmentIds", id));
    }
    const response = await apiClient.get<PagedResult<LocationDto>>("/locations", {
      params,
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

  restoreLocation: async (id: string) => {
    const response = await apiClient.put<string>(`/locations/${id}/restore`);
    return response.data;
  },
}

export const locationsQueryOptions = {
  baseKey: "locations",

    getLocationInfiniteOptions: (
      filter: Pick<GetLocationsRequest, "departmentIds" | "search" | "pageSize" | "isActive">
    ) => {
      return infiniteQueryOptions({
        queryKey: [locationsQueryOptions.baseKey,  filter ],
        queryFn: ({ pageParam }) => 
          locationsApi.getLocations({
          page: pageParam,
          pageSize: filter.pageSize,
          isActive: filter.isActive,
          search: filter.search,
          departmentIds: filter.departmentIds,
        }),
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
          pageSize: data.pages[0]?.pageSize ?? filter.pageSize,
          totalPages: data.pages[0]?.totalPages ?? 0
        })
      });
    },
};
