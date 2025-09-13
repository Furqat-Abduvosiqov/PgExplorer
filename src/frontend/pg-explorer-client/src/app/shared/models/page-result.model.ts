export interface PageResult<T> {
  items: T[];
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface PageRequest {
  pageNumber: number;
  size: number;
  sortBy?: string;
  desc?: boolean;
}