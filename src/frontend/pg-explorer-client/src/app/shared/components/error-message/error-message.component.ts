import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';

@Component({
  selector: 'app-error-message',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './error-message.component.html',
  styleUrl: './error-message.component.scss'
})
export class ErrorMessageComponent {
  @Input() message = '';
  @Input() error: any = null;
  @Input() title = '';
  @Input() details = '';
  @Input() type: 'error' | 'warning' | 'info' | 'success' = 'error';
  @Input() dismissible = false;

  @Output() dismiss = new EventEmitter<void>();

  get typeClass(): string {
    return this.type;
  }

  onDismiss(): void {
    this.dismiss.emit();
  }
}
