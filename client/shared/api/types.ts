export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
}