export enum QueryType {
  Select = 0,
  Insert = 1,
  Update = 2,
  Delete = 3,
  Create = 4,
  Drop = 5,
  Alter = 6,
  Other = 7
}

export enum QueryStatus {
  Pending = 0,
  Running = 1,
  Completed = 2,
  Failed = 3,
  Cancelled = 4
}

export interface QueryResponse {
  id: string;
  connectionId: number;
  queryBody: string;
  queryType: QueryType;
  executedAt: Date;
  executionTime: number;
  queryStatus: QueryStatus;
  errorMessage?: string;
  affectedRows?: number;
}

export interface CreateQueryRequest {
  connectionId: number;
  queryBody: string;
  queryType: QueryType;
}

export interface UpdateQueryRequest {
  connectionId: number;
  queryBody: string;
  queryType: QueryType;
}

export interface ExecutionResult {
  columns: string[];
  rows: any[][];
  affectedRows: number;
  executionTime: number;
}