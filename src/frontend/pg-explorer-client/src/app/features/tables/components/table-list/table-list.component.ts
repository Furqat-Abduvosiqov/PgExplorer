import {Component, OnDestroy, OnInit} from '@angular/core';
import {CommonModule, CurrencyPipe, DatePipe, PercentPipe} from '@angular/common';
import {ActivatedRoute, Router, RouterModule} from '@angular/router';
import {Subject} from 'rxjs';
import {finalize, takeUntil} from 'rxjs/operators';

import {TableService} from '../../../../core/services/table.service';
import {ConnectionService} from '../../../../core/services/connection.service';
import {TableInfo} from '../../../../core/models/table.model';
import {Connection} from '../../../../core/models/connection.model';
import {PageResult} from '../../../../shared/models/page-result.model';

import {SearchComponent} from '../../../../shared/components/search/search.component';
import {PaginationComponent} from '../../../../shared/components/pagination/pagination.component';
import {LoadingComponent} from '../../../../shared/components/loading/loading.component';
import {ErrorMessageComponent} from '../../../../shared/components/error-message/error-message.component';

@Component({
  selector: 'app-table-list',
  standalone: true,
  imports: [
    DatePipe,
    CurrencyPipe,
    PercentPipe,
    CommonModule,
    RouterModule,
    SearchComponent,
    PaginationComponent,
    LoadingComponent,
    ErrorMessageComponent
  ],
  templateUrl: './table-list.component.html',
  styleUrl: './table-list.component.scss'
})
export class TableListComponent implements OnInit, OnDestroy {
  tables: TableInfo[] = [] as TableInfo[];
  pageResult: PageResult<TableInfo> | null = null;
  selectedConnection: Connection | null = null;
  connectionId: number | null = null;
  databaseName: string | null = null;
  schemaName: string | null = null;

  isLoading = false;
  error = '';

  private destroy$ = new Subject<void>();
  private currentSearch = '';
  private currentPage = 1;
  private currentPageSize = 20;

  constructor(
    private tableService: TableService,
    private connectionService: ConnectionService,
    private route: ActivatedRoute,
    private router: Router
  ) {
  }

  get currentSearchTerm(): string {
    return this.currentSearch;
  }

  ngOnInit(): void {
    // Get connection info
    this.connectionService.selectedConnection$
      .pipe(takeUntil(this.destroy$))
      .subscribe(connection => {
        this.selectedConnection = connection;
      });

    // Get route parameters
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.connectionId = params['connectionId'] ? parseInt(params['connectionId'], 10) : null;
        this.databaseName = params['databaseName'] || null;
        this.schemaName = params['schemaName'] || null;

        if (this.connectionId && this.schemaName) {
          this.loadTables();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(searchTerm: string): void {
    this.currentSearch = searchTerm;
    this.currentPage = 1;
    this.loadTables();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadTables();
  }

  onPageSizeChange(pageSize: number): void {
    this.currentPageSize = pageSize;
    this.currentPage = 1;
    this.loadTables();
  }

  navigateToTableData(table: TableInfo): void {
    this.router.navigate(['/table-data'], {
      queryParams: {
        connectionId: this.connectionId,
        schemaName: table.schemaName,
        tableName: table.tableName
      }
    });
  }

  exportTable(table: TableInfo): void {
    // TODO: Implement table export functionality
    console.log('Export table:', table.tableName);
  }

  goBackToDatabases(): void {
    this.router.navigate(['/databases']);
  }

  private loadTables(): void {
    if (!this.connectionId || !this.schemaName) {
      return;
    }

    this.isLoading = true;
    this.error = '';

    const params = {
      connectionId: this.connectionId,
      schemaName: this.schemaName,
      search: this.currentSearch,
      page: this.currentPage,
      pageSize: this.currentPageSize
    };

    this.tableService.getTables(params)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.pageResult = result;
          this.tables = result.items;
        },
        error: (error) => {
          this.error = `Failed to load tables: ${error}`;
          this.tables = [];
        }
      });
  }
}
