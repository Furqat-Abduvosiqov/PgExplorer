import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {HttpClientService} from './http-client.service';
import {PageResult} from '../../shared/models/page-result.model';
import {
  DeleteRowRequest,
  ExportTableRequest,
  InsertRowRequest,
  TableData,
  TableInfo,
  UpdateRowRequest
} from '../models/table.model';

@Injectable({
  providedIn: 'root'
})
export class TableService {
  constructor(private httpClient: HttpClientService) {
  }

  getTables(params: {
    connectionId: number;
    schemaName: string;
    search?: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    desc?: boolean;
  }): Observable<PageResult<TableInfo>> {
    const queryParams = {
      connectionId: params.connectionId,
      schemaName: params.schemaName,
      search: params.search || '',
      page: params.page || 1,
      pageSize: params.pageSize || 20,
      sortBy: params.sortBy,
      desc: params.desc || false
    };

    return this.httpClient.get<PageResult<TableInfo>>('/tables', queryParams);
  }

  getTableData(params: {
    connectionId: number;
    schemaName: string;
    tableName: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    desc?: boolean;
  }): Observable<TableData> {
    const queryParams = {
      connectionId: params.connectionId,
      schemaName: params.schemaName,
      tableName: params.tableName,
      page: params.page || 1,
      pageSize: params.pageSize || 100,
      sortBy: params.sortBy,
      desc: params.desc || false
    };

    return this.httpClient.get<TableData>('/tables/data', queryParams);
  }

  insertRow(request: InsertRowRequest): Observable<void> {
    return this.httpClient.post<void>('/tables/rows', request);
  }

  updateRow(request: UpdateRowRequest): Observable<void> {
    return this.httpClient.put<void>('/tables/rows', request);
  }

  deleteRow(request: DeleteRowRequest): Observable<void> {
    const queryParams = {
      connectionId: request.connectionId,
      schemaName: request.schemaName,
      tableName: request.tableName,
      whereClause: request.whereClause
    };

    return this.httpClient.delete<void>('/tables/rows', queryParams);
  }

  exportTable(request: ExportTableRequest): Observable<Blob> {
    const queryParams = {
      connectionId: request.connectionId,
      schemaName: request.schemaName,
      tableName: request.tableName,
      format: request.format,
      includeHeaders: request.includeHeaders || true
    };

    // Note: This should return a Blob for file download
    return this.httpClient.get<Blob>('/tables/export', queryParams);
  }
}
