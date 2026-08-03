export type DepartmentShortDto = {
  id: string;
  name: string;
  identifier: string;
  path: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type GetDepartmentsRequest = {
  search?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
};

export type DepartmentWithChildrenDto = {
  id: string;
  parentId: string | null;
  name: string;
  identifier: string;
  path: string;
  depth: number;
  isActive: boolean;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string;
  children: DepartmentWithChildrenDto[];
  hasChildren: boolean;
};

export type GetDepartmentRootsRequest = {
  page?: number;
  size?: number;
  prefetch?: number;
};

export type GetDepartmentChildrenRequest = {
  page?: number;
  size?: number;
};
