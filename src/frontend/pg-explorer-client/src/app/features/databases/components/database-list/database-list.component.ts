import {Component, OnDestroy, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router, RouterModule} from '@angular/router';
import {Subject} from 'rxjs';
import {finalize, takeUntil} from 'rxjs/operators';

import {DatabaseService} from '../../../../core/services/database.service';
import {ConnectionService} from '../../../../core/services/connection.service';
import {DatabaseInfo, SchemaInfo} from '../../../../core/models/database.model';
import {Connection} from '../../../../core/models/connection.model';
import {PageResult} from '../../../../shared/models/page-result.model';

import {SearchComponent} from '../../../../shared/components/search/search.component';
import {PaginationComponent} from '../../../../shared/components/pagination/pagination.component';
import {LoadingComponent} from '../../../../shared/components/loading/loading.component';
import {ErrorMessageComponent} from '../../../../shared/components/error-message/error-message.component';
import {CreateDatabaseModalComponent} from '../create-database-modal/create-database-modal.component';
import {DropDatabaseModalComponent} from '../drop-database-modal/drop-database-modal.component';

@Component({
  selector: 'app-database-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    SearchComponent,
    PaginationComponent,
    LoadingComponent,
    ErrorMessageComponent,
    CreateDatabaseModalComponent,
    DropDatabaseModalComponent
  ],
  templateUrl: './database-list.component.html',
  styleUrl: './database-list.component.scss'
})
export class DatabaseListComponent implements OnInit, OnDestroy {
  databases: DatabaseInfo[] = [];
  schemas: SchemaInfo[] = [];
  pageResult: PageResult<DatabaseInfo> | null = null;
  schemaPageResult: PageResult<SchemaInfo> | null = null;
  selectedConnection: Connection | null = null;
  selectedDatabase: DatabaseInfo | null = null;

  isLoading = false;
  isSchemasLoading = false;
  error = '';
  schemaError = '';

  showCreateDatabaseModal = false;
  showDropDatabaseModal = false;
  databaseToDelete: DatabaseInfo | null = null;

  private destroy$ = new Subject<void>();
  private currentSearch = '';
  private currentSchemaSearch = '';
  private currentPage = 1;
  private currentSchemaPage = 1;
  private currentPageSize = 20;
  private currentSchemaPageSize = 20;

  constructor(
    private databaseService: DatabaseService,
    private connectionService: ConnectionService,
    private router: Router
  ) {
  }

  ngOnInit(): void {
    this.connectionService.selectedConnection$
      .pipe(takeUntil(this.destroy$))
      .subscribe(connection => {
        this.selectedConnection = connection;
        if (connection) {
          this.loadDatabases();
        } else {
          this.databases = [];
          this.schemas = [];
          this.selectedDatabase = null;
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
    this.loadDatabases();
  }

  onSchemaSearch(searchTerm: string): void {
    this.currentSchemaSearch = searchTerm;
    this.currentSchemaPage = 1;
    this.loadSchemas();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadDatabases();
  }

  onSchemaPageChange(page: number): void {
    this.currentSchemaPage = page;
    this.loadSchemas();
  }

  onPageSizeChange(pageSize: number): void {
    this.currentPageSize = pageSize;
    this.currentPage = 1;
    this.loadDatabases();
  }

  onSchemaPageSizeChange(pageSize: number): void {
    this.currentSchemaPageSize = pageSize;
    this.currentSchemaPage = 1;
    this.loadSchemas();
  }

  selectDatabase(database: DatabaseInfo): void {
    this.selectedDatabase = database;
    this.schemas = [];
    this.schemaPageResult = null;
    this.loadSchemas();
  }

  openCreateDatabaseModal(): void {
    this.showCreateDatabaseModal = true;
  }

  closeCreateDatabaseModal(): void {
    this.showCreateDatabaseModal = false;
  }

  openDropDatabaseModal(database: DatabaseInfo): void {
    this.databaseToDelete = database;
    this.showDropDatabaseModal = true;
  }

  closeDropDatabaseModal(): void {
    this.showDropDatabaseModal = false;
    this.databaseToDelete = null;
  }

  onDatabaseCreated(): void {
    this.closeCreateDatabaseModal();
    this.loadDatabases();
  }

  onDatabaseDropped(): void {
    this.closeDropDatabaseModal();
    this.loadDatabases();
    if (this.selectedDatabase?.name === this.databaseToDelete?.name) {
      this.selectedDatabase = null;
      this.schemas = [];
      this.schemaPageResult = null;
    }
  }

  navigateToTables(schemaName: string): void {
    if (this.selectedDatabase) {
      this.router.navigate(['/tables'], {
        queryParams: {
          connectionId: this.selectedConnection?.id,
          databaseName: this.selectedDatabase.name,
          schemaName: schemaName
        }
      });
    }
  }

  private loadDatabases(): void {
    if (!this.selectedConnection) {
      return;
    }

    this.isLoading = true;
    this.error = '';

    const params = {
      connectionId: this.selectedConnection.id,
      search: this.currentSearch,
      page: this.currentPage,
      pageSize: this.currentPageSize
    };

    this.databaseService.getDatabases(params)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.pageResult = result;
          this.databases = result.items;
        },
        error: (error) => {
          this.error = `Failed to load databases: ${error}`;
          this.databases = [];
        }
      });
  }

  private loadSchemas(): void {
    if (!this.selectedConnection || !this.selectedDatabase) {
      return;
    }

    this.isSchemasLoading = true;
    this.schemaError = '';

    const params = {
      connectionId: this.selectedConnection.id,
      databaseName: this.selectedDatabase.name,
      search: this.currentSchemaSearch,
      page: this.currentSchemaPage,
      pageSize: this.currentSchemaPageSize
    };

    this.databaseService.getSchemas(params)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSchemasLoading = false)
      )
      .subscribe({
        next: (result) => {
          this.schemaPageResult = result;
          this.schemas = result.items;
        },
        error: (error) => {
          this.schemaError = `Failed to load schemas: ${error}`;
          this.schemas = [];
        }
      });
  }
}
