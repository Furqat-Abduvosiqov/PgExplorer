import {Injectable} from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import {HttpClientService} from './http-client.service';
import {PageResult} from '../../shared/models/page-result.model';
import {
  Connection,
  CreateConnectionRequest,
  TestConnectionRequest,
  TestConnectionResponse,
  UpdateConnectionRequest
} from '../models/connection.model';

@Injectable({
  providedIn: 'root'
})
export class ConnectionService {
  private selectedConnectionSubject = new BehaviorSubject<Connection | null>(null);
  public selectedConnection$ = this.selectedConnectionSubject.asObservable();

  constructor(private httpClient: HttpClientService) {
  }

  getConnections(params?: {
    search?: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    desc?: boolean;
  }): Observable<PageResult<Connection>> {
    const queryParams = {
      search: params?.search || '',
      page: params?.page || 1,
      pageSize: params?.pageSize || 20,
      sortBy: params?.sortBy,
      desc: params?.desc || false
    };

    return this.httpClient.get<PageResult<Connection>>('/connections', queryParams);
  }

  getConnectionById(id: number): Observable<Connection> {
    return this.httpClient.get<Connection>(`/connections/${id}`);
  }

  createConnection(request: CreateConnectionRequest): Observable<Connection> {
    return this.httpClient.post<Connection>('/connections', request);
  }

  updateConnection(id: number, request: UpdateConnectionRequest): Observable<Connection> {
    return this.httpClient.put<Connection>(`/connections/${id}`, request);
  }

  deleteConnection(id: number): Observable<void> {
    return this.httpClient.delete<void>(`/connections/${id}`);
  }

  testConnection(request: TestConnectionRequest): Observable<TestConnectionResponse> {
    return this.httpClient.post<TestConnectionResponse>('/connections/test', request);
  }

  getConnectionQueries(connectionId: number, params?: {
    search?: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    desc?: boolean;
  }): Observable<PageResult<any>> {
    const queryParams = {
      connectionId,
      search: params?.search || '',
      page: params?.page || 1,
      pageSize: params?.pageSize || 20,
      sortBy: params?.sortBy,
      desc: params?.desc || false
    };

    return this.httpClient.get<PageResult<any>>(`/connections/${connectionId}/queries`, queryParams);
  }

  setSelectedConnection(connection: Connection | null): void {
    this.selectedConnectionSubject.next(connection);
  }

  getSelectedConnection(): Connection | null {
    return this.selectedConnectionSubject.value;
  }
}
