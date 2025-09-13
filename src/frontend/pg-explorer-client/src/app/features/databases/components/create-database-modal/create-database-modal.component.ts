import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {Subject} from 'rxjs';
import {finalize, takeUntil} from 'rxjs/operators';

import {DatabaseService} from '../../../../core/services/database.service';
import {CreateDatabaseRequest} from '../../../../core/models/database.model';

import {LoadingComponent} from '../../../../shared/components/loading/loading.component';
import {ErrorMessageComponent} from '../../../../shared/components/error-message/error-message.component';

@Component({
  selector: 'app-create-database-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LoadingComponent,
    ErrorMessageComponent
  ],
  templateUrl: './create-database-modal.component.html',
  styleUrl: './create-database-modal.component.scss'
})
export class CreateDatabaseModalComponent implements OnInit, OnDestroy {
  @Input() connectionId!: number;
  @Output() close = new EventEmitter<void>();
  @Output() created = new EventEmitter<void>();

  createForm: FormGroup;
  isSubmitting = false;
  error = '';

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private databaseService: DatabaseService
  ) {
    this.createForm = this.createFormMethod();
  }

  ngOnInit(): void {
    // Focus on first input when modal opens
    setTimeout(() => {
      const firstInput = document.querySelector('.modal input') as HTMLInputElement;
      if (firstInput) {
        firstInput.focus();
      }
    }, 100);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    if (this.createForm.valid) {
      this.isSubmitting = true;
      this.error = '';

      const request: CreateDatabaseRequest = {
        connectionId: this.connectionId,
        name: this.createForm.value.name,
        owner: this.createForm.value.owner || undefined,
        template: this.createForm.value.template || undefined,
        encoding: this.createForm.value.encoding || undefined,
        locale: this.createForm.value.locale || undefined,
        tablespace: this.createForm.value.tablespace || undefined
      };

      this.databaseService.createDatabase(request)
        .pipe(
          takeUntil(this.destroy$),
          finalize(() => this.isSubmitting = false)
        )
        .subscribe({
          next: () => {
            this.created.emit();
          },
          error: (error) => {
            this.error = `Failed to create database: ${error}`;
          }
        });
    }
  }

  onCancel(): void {
    this.close.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close.emit();
    }
  }

  private createFormMethod(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.pattern(/^[a-zA-Z_][a-zA-Z0-9_]*$/)]],
      owner: [''],
      template: [''],
      encoding: [''],
      locale: [''],
      tablespace: ['']
    });
  }
}
