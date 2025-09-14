import {CommonModule} from "@angular/common";
import {Component, Input} from '@angular/core';

@Component({
  selector: 'app-success-message',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './success-message.component.html',
  styleUrl: './success-message.component.scss'

})
export class SuccessMessageComponent {
  @Input() message = '';
  @Input() duration = 3000;
  @Input() type: 'success' | 'info' | 'warn' | 'error' = 'success';

  visible = false;

  show(): void {
    // start show animation on next tick
    requestAnimationFrame(() => this.visible = true);
  }

  hide(): void {
    this.visible = false;
  }
}
