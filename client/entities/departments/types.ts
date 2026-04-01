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