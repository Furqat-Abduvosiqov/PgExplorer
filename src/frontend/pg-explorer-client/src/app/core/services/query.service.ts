import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {HttpClientService} from './http-client.service';
import {PageResult} from '../../shared/models/page-result.model';
import {CreateQueryRequest, ExecutionResult, QueryResponse, UpdateQueryRequest} from '../models/query.model';

@Injectable({
  providedIn: 'root'
})
export class QueryService {
  constructor(private httpClient: HttpClientService) {
  }

  getQueries(params?: {
    search?: string;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    desc?: boolean;
  }): Observable<PageResult<QueryResponse>> {
    const queryParams = {
      search: params?.search || '',
      page: params?.page || 1,
      pageSize: params?.pageSize || 20,
      sortBy: params?.sortBy,
      desc: params?.desc || false
    };

    return this.httpClient.get<PageResult<QueryResponse>>('/queries', queryParams);
  }

  getQueryById(id: string): Observable<QueryResponse> {
    return this.httpClient.get<QueryResponse>(`/queries/${id}`);
  }

  createQuery(request: CreateQueryRequest): Observable<QueryResponse> {
    return this.httpClient.post<QueryResponse>('/queries', request);
  }

  updateQuery(id: string, request: UpdateQueryRequest): Observable<QueryResponse> {
    return this.httpClient.put<QueryResponse>(`/queries/${id}`, request);
  }

  deleteQuery(id: string): Observable<void> {
    return this.httpClient.delete<void>(`/queries/${id}`);
  }

  executeQuery(request: CreateQueryRequest): Observable<ExecutionResult> {
    return this.httpClient.post<ExecutionResult>('/queries/execute', request);
  }
}
