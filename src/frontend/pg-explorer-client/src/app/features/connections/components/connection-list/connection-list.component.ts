import {Component, OnDestroy, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterModule} from '@angular/router';
import {Subject} from 'rxjs';
import {finalize, takeUntil} from 'rxjs/operators';

import {ConnectionService} from '../../../../core/services/connection.service';
import {Connection} from '../../../../core/models/connection.model';
import {PageResult} from '../../../../shared/models/page-result.model';

import {SearchComponent} from '../../../../shared/components/search/search.component';
import {PaginationComponent} from '../../../../shared/components/pagination/pagination.component';
import {LoadingComponent} from '../../../../shared/components/loading/loading.component';
import {ErrorMessageComponent} from '../../../../shared/components/error-message/error-message.component';

@Component({
  selector: 'app-connection-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    SearchComponent,
    PaginationComponent,
    LoadingComponent,
    ErrorMessageComponent
  ],
  providers: [],
  templateUrl: './connection-list.component.html',
  styleUrl: './connection-list.component.scss'
})

export class ConnectionListComponent implements OnInit, OnDestroy {
  connections: Connection[] = [];
  pageResult: PageResult<Connection> | null = null;
  selectedConnection: Connection | null = null;

  isLoading = false;
  error = '';
  testingConnectionId: number | null = null;
  deletingConnectionId: number | null = null;

  private destroy$ = new Subject<void>();
  private currentSearch = '';
  private currentPage = 1;
  private currentPageSize = 20;

  constructor(private connectionService: ConnectionService) {
  }

  ngOnInit(): void {
    this.selectedConnection = this.connectionService.getSelectedConnection();
    this.loadConnections();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(searchTerm: string): void {
    this.currentSearch = searchTerm;
    this.currentPage = 1;
    this.loadConnections();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadConnections();
  }

  onPageSizeChange(pageSize: number): void {
    this.currentPageSize = pageSize;
    this.currentPage = 1;
    this.loadConnections();
  }

  selectConnection(connection: Connection): void {
    this.selectedConnection = connection;
    this.connectionService.setSelectedConnection(connection);
  }

  testConnection(connection: Connection): void {
    this.testingConnectionId = connection.id;

    const testRequest = {
      host: connection.host,
      port: connection.port,
      databaseName: connection.databaseName,
      username: connection.username,
      password: connection.password || ''
    };

    this.connectionService.testConnection(testRequest)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.testingConnectionId = null)
      )
      .subscribe({
        next: (result) => {
          if (result.isSuccessful) {
            // Show success message
            console.log('Connection test successful:', result.message);
          } else {
            this.error = result.message;
          }
        },
        error: (error) => {
          this.error = `Connection test failed: ${error}`;
        }
      });
  }

  deleteConnection(connection: Connection): void {
    if (!confirm(`Are you sure you want to delete connection "${connection.name}"?`)) {
      return;
    }

    this.deletingConnectionId = connection.id;

    this.connectionService.deleteConnection(connection.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.deletingConnectionId = null)
      )
      .subscribe({
        next: () => {
          this.loadConnections();
          if (this.selectedConnection?.id === connection.id) {
            this.selectedConnection = null;
            this.connectionService.setSelectedConnection(null);
          }
        },
        error: (error) => {
          this.error = `Failed to delete connection: ${error}`;
        }
      });
  }

  private loadConnections(): void {
    this.isLoading = true;
    this.error = '';

    const params = {
      search: this.currentSearch,
      page: this.currentPage,
      pageSize: this.currentPageSize
    };

    this.connectionService.getConnections(params)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.pageResult = result;
          this.connections = result.items;
        },
        error: (error) => {
          this.error = `Failed to load connections: ${error}`;
          this.connections = [];
        }
      });
  }
}
