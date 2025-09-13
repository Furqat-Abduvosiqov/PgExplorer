export interface DatabaseInfo {
  name: string;
  owner: string;
  encoding: string;
  collate: string;
  ctype: string;
  isTemplate: boolean;
  allowConnections: boolean;
  connectionLimit: number;
  size: string;
}

export interface SchemaInfo {
  name: string;
  owner: string;
  privileges: string[];
}

export interface CreateDatabaseRequest {
  connectionId: number;
  name: string;
  owner?: string;
  template?: string;
  encoding?: string;
  locale?: string;
  tablespace?: string;
}

export interface DropDatabaseRequest {
  connectionId: number;
  name: string;
  force?: boolean;
}