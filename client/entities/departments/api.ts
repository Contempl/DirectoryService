import { PagedResult } from "@/shared/api/types";
import { DepartmentShortDto, DepartmentWithChildrenDto, GetDepartmentChildrenRequest, GetDepartmentRootsRequest } from "./types";
import { apiClient } from "@/shared/api/axios-instance";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import type { Envelope } from "@/shared/api/envelope";

export type GetDepartmentsRequest = {
  departmentIds?: string[];
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

  getDepartmentsByIds: async (departmentIds: string[]) => {
    const params = new URLSearchParams({ page: "1", pageSize: String(departmentIds.length) });
    departmentIds.forEach((id) => params.append("departmentIds", id));
    const response = await apiClient.get<PagedResult<DepartmentShortDto>>("/departments", { params });
    return response.data.items;
  },

  getRoots: async (params?: GetDepartmentRootsRequest) => {
    const response = await apiClient.get<DepartmentWithChildrenDto[]>("/departments/tree", {
      params,
    });
    return response.data;
  },

  getChildren: async (parentId: string, params?: GetDepartmentChildrenRequest) => {
    const response = await apiClient.get<DepartmentWithChildrenDto[]>(
      `/departments/${parentId}/children`,
      { params }
    );
    return response.data;
  },

  getDescendantIds: async (departmentId: string) => {
    const response = await apiClient.get<Envelope<string[]>>(
      `/departments/${departmentId}/descendant-ids`
    );
    return response.data.result ?? [];
  },

  moveDepartment: async (departmentId: string, parentId: string | null) => {
    const response = await apiClient.put<Envelope<string>>(
      `/departments/${departmentId}/parent`,
      undefined,
      { params: parentId === null ? undefined : { parentId } }
    );
    return response.data.result;
  },

  toggleActivity: async (departmentId: string, isActive: boolean) => {
    const response = await apiClient.put<Envelope<string>>(
      `/departments/${departmentId}/activity`,
      { isActive }
    );
    return response.data.result;
  },
};



export const departmentsQueryOptions = {
  baseKey: "departments",

  byIds: (departmentIds: string[]) =>
    queryOptions({
      queryKey: [departmentsQueryOptions.baseKey, "byIds", departmentIds],
      queryFn: () => departmentsApi.getDepartmentsByIds(departmentIds),
      enabled: departmentIds.length > 0,
    }),

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
