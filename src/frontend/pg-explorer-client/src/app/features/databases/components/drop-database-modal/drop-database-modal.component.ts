import {Component, EventEmitter, Input, OnDestroy, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {Subject} from 'rxjs';
import {finalize, takeUntil} from 'rxjs/operators';

import {DatabaseService} from '../../../../core/services/database.service';
import {DatabaseInfo, DropDatabaseRequest} from '../../../../core/models/database.model';

import {LoadingComponent} from '../../../../shared/components/loading/loading.component';
import {ErrorMessageComponent} from '../../../../shared/components/error-message/error-message.component';

@Component({
  selector: 'app-drop-database-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LoadingComponent,
    ErrorMessageComponent
  ],
  templateUrl: './drop-database-modal.component.html',
  styleUrl: './drop-database-modal.component.scss'
})
export class DropDatabaseModalComponent implements OnDestroy {
  @Input() database!: DatabaseInfo | null;
  @Input() connectionId!: number;
  @Output() close = new EventEmitter<void>();
  @Output() dropped = new EventEmitter<void>();

  confirmationText = '';
  forceDelete = false;
  isSubmitting = false;
  error = '';

  private destroy$ = new Subject<void>();

  constructor(private databaseService: DatabaseService) {
  }

  get isConfirmationValid(): boolean {
    return this.confirmationText === this.database?.name;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    if (!this.database || !this.isConfirmationValid) {
      return;
    }

    this.isSubmitting = true;
    this.error = '';

    const request: DropDatabaseRequest = {
      connectionId: this.connectionId,
      name: this.database.name,
      force: this.forceDelete
    };

    this.databaseService.dropDatabase(request)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.isSubmitting = false)
      )
      .subscribe({
        next: () => {
          this.dropped.emit();
        },
        error: (error) => {
          this.error = `Failed to drop database: ${error}`;
        }
      });
  }

  onCancel(): void {
    this.close.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close.emit();
    }
  }
}
