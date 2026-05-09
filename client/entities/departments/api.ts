import { PagedResult } from "@/shared/api/types";
import { DepartmentShortDto } from "./types";
import { apiClient } from "@/shared/api/axios-instance";
import { infiniteQueryOptions } from "@tanstack/react-query";

export type GetDepartmentsRequest = {
  search?: string;
  isActive?: boolean;
  pageSize: number;
  page: number;
};



export const departmentsApi = {
  getDepartments: async (query?: GetDepartmentsRequest) => {
    const response = await apiClient.get<PagedResult<DepartmentShortDto>>("/departments", {
      params: query,
    });
    return response.data;
  },
};



export const departmentsQueryOptions = {
  baseKey: "departments",

  getDepartmentsInfiniteOptions: (filter: GetDepartmentsRequest) => {
    return infiniteQueryOptions({
      queryKey: [departmentsQueryOptions.baseKey, filter],
      queryFn: ({ pageParam }) =>
        departmentsApi.getDepartments({
          page: pageParam,
          pageSize: filter.pageSize,
          search: filter.search,
          isActive: filter.isActive,
        }),
      initialPageParam: 1,
      getNextPageParam: (lastPage) => {
        if (!lastPage.items || lastPage.page >= lastPage.totalPages) {
          return undefined;
        }
        return lastPage.page + 1;
      },
      select: (data): PagedResult<DepartmentShortDto> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? filter.pageSize,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },
};