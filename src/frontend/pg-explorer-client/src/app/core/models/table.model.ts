export interface TableInfo {
  tableName: string;
  schemaName: string;
  tableType: string;
  owner: string;
  hasIndexes: boolean;
  hasRules: boolean;
  hasTriggers: boolean;
  hasRowSecurity: boolean;
  estimatedRowCount: number;
}

export interface ColumnInfo {
  columnName: string;
  dataType: string;
  isNullable: boolean;
  defaultValue?: string;
  maxLength?: number;
  precision?: number;
  scale?: number;
  isPrimaryKey: boolean;
  isForeignKey: boolean;
  isUnique: boolean;
  ordinalPosition: number;
}

export interface TableData {
  columns: ColumnInfo[];
  rows: any[];
  totalRows: number;
  hasMoreRows: boolean;
}

export interface InsertRowRequest {
  connectionId: number;
  schemaName: string;
  tableName: string;
  data: Record<string, any>;
}

export interface UpdateRowRequest {
  connectionId: number;
  schemaName: string;
  tableName: string;
  whereClause: string;
  data: Record<string, any>;
}

export interface DeleteRowRequest {
  connectionId: number;
  schemaName: string;
  tableName: string;
  whereClause: string;
}

export interface ExportTableRequest {
  connectionId: number;
  schemaName: string;
  tableName: string;
  format: 'csv' | 'json' | 'xml';
  includeHeaders?: boolean;
}