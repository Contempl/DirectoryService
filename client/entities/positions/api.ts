import { apiClient } from "@/shared/api/axios-instance";
import { infiniteQueryOptions } from "@tanstack/react-query";
import { PagedResult } from "@/shared/api/types";
import { PositionDto, GetPositionsRequest, CreatePositionRequest, UpdatePositionRequest } from "./types";

export const positionsApi = {
  getPositions: async (query?: GetPositionsRequest) => {
  const response = await apiClient.get<PagedResult<PositionDto>>("/positions", {
    params: query,
    paramsSerializer: (params) => {
      const searchParams = new URLSearchParams();
      Object.entries(params).forEach(([key, value]) => {
        if (Array.isArray(value)) {
          value.forEach((v) => searchParams.append(key, v));
        } else if (value !== undefined && value !== null) {
          searchParams.append(key, value);
        }
      });
      return searchParams.toString();
    },
  });
  return response.data;
},

  getPositionById: async (id: string) => {
    const response = await apiClient.get<PositionDto>(`/positions/${id}`);
    return response.data;
  },

  createPosition: async (request: CreatePositionRequest) => {
    const response = await apiClient.post("/positions", request);
    return response.data;
  },

  updatePosition: async (id: string, request: UpdatePositionRequest) => {
    const response = await apiClient.put(`/positions/${id}`, request);
    return response.data;
  },

  deletePosition: async (id: string) => {
    const response = await apiClient.delete(`/positions/${id}`);
    return response.data;
  },
};

export const positionsQueryOptions = {
  baseKey: "positions",

  getPositionsInfiniteOptions: (filter: GetPositionsRequest) => {
    return infiniteQueryOptions({
      queryKey: [positionsQueryOptions.baseKey, filter],
      queryFn: ({ pageParam }) =>
        positionsApi.getPositions({
          page: pageParam,
          pageSize: filter.pageSize,
          search: filter.search,
          isActive: filter.isActive,
          departmentIds: filter.departmentIds,
        }),
      initialPageParam: 1,
      getNextPageParam: (lastPage) => {
        if (!lastPage.items || lastPage.page >= lastPage.totalPages) {
          return undefined;
        }
        return lastPage.page + 1;
      },
      select: (data): PagedResult<PositionDto> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? filter.pageSize,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },
};