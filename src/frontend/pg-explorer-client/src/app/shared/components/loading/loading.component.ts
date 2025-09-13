import {Component, Input} from '@angular/core';
import {CommonModule} from '@angular/common';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading.component.html',
  styleUrl: './loading.component.scss'
})
export class LoadingComponent {
  @Input() message = '';
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() overlay = false;

  get containerClass(): string {
    return this.overlay ? 'overlay' : 'inline';
  }

  get spinnerClass(): string {
    return this.size;
  }

  get textClass(): string {
    return this.size;
  }
}
