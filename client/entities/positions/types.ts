export type PositionDto = {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  departmentsCount: number;
  createdAt: string;
  updatedAt: string;
  deletedAt: string | null;
};

export type GetPositionsRequest = {
  search?: string;
  isActive?: boolean;
  departmentIds?: string[];
  page: number;
  pageSize: number;
};

export type CreatePositionRequest = {
  name: string;
  description?: string | null;
  departmentIds: string[];
};

export type UpdatePositionRequest = {
  name: string;
  description?: string | null;
};