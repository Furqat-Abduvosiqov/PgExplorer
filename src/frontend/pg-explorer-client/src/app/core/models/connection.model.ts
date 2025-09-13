export interface Connection {
  id: number;
  name: string;
  host: string;
  port: number;
  databaseName: string;
  username: string;
  password?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateConnectionRequest {
  name: string;
  host: string;
  port: number;
  databaseName: string;
  username: string;
  password: string;
}

export interface UpdateConnectionRequest {
  name: string;
  host: string;
  port: number;
  databaseName: string;
  username: string;
  password?: string;
}

export interface TestConnectionRequest {
  host: string;
  port: number;
  databaseName: string;
  username: string;
  password: string;
}

export interface TestConnectionResponse {
  isSuccessful: boolean;
  message: string;
}
