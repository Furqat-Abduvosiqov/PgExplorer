import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {HttpClientService} from './http-client.service';
import {PageResult} from '../../shared/models/page-result.model';
import {CreateDatabaseRequest, DatabaseInfo, DropDatabaseRequest, SchemaInfo} from '../models/database.model';

@Injectable({
  providedIn: 'root'
})
export class DatabaseService {
  constructor(private httpClient: HttpClientService) {
  }

  getDatabases(params: {
    connectionId: number;
    search?: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    desc?: boolean;
  }): Observable<PageResult<DatabaseInfo>> {
    const queryParams = {
      connectionId: params.connectionId,
      search: params.search || '',
      page: params.page || 1,
      pageSize: params.pageSize || 20,
      sortBy: params.sortBy,
      desc: params.desc || false
    };

    return this.httpClient.get<PageResult<DatabaseInfo>>('/databases', queryParams);
  }

  getSchemas(params: {
    connectionId: number;
    databaseName: string;
    search?: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    desc?: boolean;
  }): Observable<PageResult<SchemaInfo>> {
    const {databaseName, ...queryParams} = params;
    const finalParams = {
      connectionId: params.connectionId,
      search: params.search || '',
      page: params.page || 1,
      pageSize: params.pageSize || 20,
      sortBy: params.sortBy,
      desc: params.desc || false
    };

    return this.httpClient.get<PageResult<SchemaInfo>>(
      `/databases/${databaseName}/schemas`,
      finalParams
    );
  }

  createDatabase(request: CreateDatabaseRequest): Observable<void> {
    return this.httpClient.post<void>('/databases', request);
  }

  dropDatabase(request: DropDatabaseRequest): Observable<void> {
    return this.httpClient.delete<void>(
      `/databases/${request.name}?connectionId=${request.connectionId}&force=${request.force || false}`
    );
  }
}
