import {Component, OnDestroy, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {ActivatedRoute, Router} from '@angular/router';
import {Subject} from 'rxjs';
import {finalize, takeUntil} from 'rxjs/operators';

import {ConnectionService} from '../../../../core/services/connection.service';
import {
  CreateConnectionRequest,
  TestConnectionRequest,
  UpdateConnectionRequest
} from '../../../../core/models/connection.model';

import {LoadingComponent} from '../../../../shared/components/loading/loading.component';
import {ErrorMessageComponent} from '../../../../shared/components/error-message/error-message.component';

@Component({
  selector: 'app-connection-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LoadingComponent,
    ErrorMessageComponent
  ],
  templateUrl: './connection-form.component.html',
  styleUrl: './connection-form.component.scss'
})

export class ConnectionFormComponent implements OnInit, OnDestroy {
  connectionForm: FormGroup;
  isEditMode = false;
  connectionId: number | null = null;

  isLoading = false;
  isSubmitting = false;
  isTesting = false;
  error = '';
  loadingMessage = '';
  testResult: { isSuccessful: boolean; message: string } | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private connectionService: ConnectionService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.connectionForm = this.createForm();
  }

  ngOnInit(): void {
    this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const id = params['id'];
      if (id && id !== 'new') {
        this.isEditMode = true;
        this.connectionId = parseInt(id, 10);
        this.loadConnection();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    if (this.connectionForm.valid) {
      this.isSubmitting = true;
      this.error = '';

      if (this.isEditMode && this.connectionId) {
        this.updateConnection();
      } else {
        this.createConnection();
      }
    }
  }

  testConnection(): void {
    if (this.connectionForm.valid) {
      this.isTesting = true;
      this.error = '';
      this.testResult = null;

      const testRequest: TestConnectionRequest = {
        host: this.connectionForm.value.host,
        port: this.connectionForm.value.port,
        databaseName: this.connectionForm.value.database,
        username: this.connectionForm.value.username,
        password: this.connectionForm.value.password
      };

      this.connectionService.testConnection(testRequest)
        .pipe(
          takeUntil(this.destroy$),
          finalize(() => this.isTesting = false)
        )
        .subscribe({
          next: (result) => {
            this.testResult = result;
            if (!result.isSuccessful) {
              this.error = result.message;
            }
          },
          error: (error) => {
            this.error = `Connection test failed: ${error}`;
          }
        });
    }
  }

  goBack(): void {
    this.router.navigate(['/connections']);
  }

  private createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      host: ['localhost', Validators.required],
      port: [5432, [Validators.required, Validators.min(1), Validators.max(65535)]],
      database: ['postgres', Validators.required],
      username: ['postgres', Validators.required],
      password: ['', this.isEditMode ? [] : [Validators.required]]
    });
  }

  private loadConnection(): void {
    if (!this.connectionId) return;

    this.isLoading = true;
    this.loadingMessage = 'Loading connection details...';

    this.connectionService.getConnectionById(this.connectionId)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (connection) => {
          this.connectionForm.patchValue({
            name: connection.name,
            host: connection.host,
            port: connection.port,
            database: connection.databaseName,
            username: connection.username,
            password: '' // Don't populate password for security
          });

          // Update password validation for edit mode
          this.connectionForm.get('password')?.clearValidators();
          this.connectionForm.get('password')?.updateValueAndValidity();
        },
        error: (error) => {
          this.error = `Failed to load connection: ${error}`;
        }
      });
  }

  private createConnection(): void {
    const request: CreateConnectionRequest = {
      name: this.connectionForm.value.name,
      host: this.connectionForm.value.host,
      port: this.connectionForm.value.port,
      databaseName: this.connectionForm.value.database,
      username: this.connectionForm.value.username,
      password: this.connectionForm.value.password
    };

    this.connectionService.createConnection(request)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSubmitting = false)
      )
      .subscribe({
        next: (connection) => {
          this.router.navigate(['/connections']);
        },
        error: (error) => {
          this.error = `Failed to create connection: ${error}`;
        }
      });
  }

  private updateConnection(): void {
    if (!this.connectionId) return;

    const request: UpdateConnectionRequest = {
      name: this.connectionForm.value.name,
      host: this.connectionForm.value.host,
      port: this.connectionForm.value.port,
      databaseName: this.connectionForm.value.database,
      username: this.connectionForm.value.username
    };

    // Only include password if it's provided
    if (this.connectionForm.value.password) {
      request.password = this.connectionForm.value.password;
    }

    this.connectionService.updateConnection(this.connectionId, request)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSubmitting = false)
      )
      .subscribe({
        next: (connection) => {
          this.router.navigate(['/connections']);
        },
        error: (error) => {
          this.error = `Failed to update connection: ${error}`;
        }
      });
  }
}
