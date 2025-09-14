import {ApplicationRef, createComponent, Injectable, Injector} from '@angular/core';
import {SuccessMessageComponent} from '../components/success-message/success-message.component';

export interface SnackbarOptions {
  duration?: number; // ms
  type?: 'success' | 'info' | 'warn' | 'error';
}

@Injectable({providedIn: 'root'})
export class SuccessMessageService {
  private queue: { message: string; options?: SnackbarOptions }[] = [];
  private showing = false;

  constructor(private appRef: ApplicationRef, private injector: Injector) {
  }

  show(message: string, options?: SnackbarOptions) {
    this.queue.push({message, options});
    this.processQueue();
  }

  private async processQueue() {
    if (this.showing) return;
    const item = this.queue.shift();
    if (!item) return;

    this.showing = true;
    await this.showOnce(item.message, item.options);
    this.showing = false;

    // next in queue
    if (this.queue.length > 0) {
      this.processQueue();
    }
  }

  private showOnce(message: string, options?: SnackbarOptions): Promise<void> {
    return new Promise<void>((resolve) => {
      // create standalone component dynamically
      const compRef = createComponent<SuccessMessageComponent>(SuccessMessageComponent, {
        environmentInjector: this.appRef.injector
      });

      // configure
      compRef.setInput('message', message);
      compRef.setInput('duration', options?.duration ?? 3000);
      compRef.setInput('type', options?.type ?? 'success');

      // attach to DOM
      this.appRef.attachView(compRef.hostView);
      const el = (compRef.location.nativeElement as HTMLElement);
      document.body.appendChild(el);

      // show
      compRef.instance.show();

      const duration = options?.duration ?? 3000;
      // hide after duration
      const timeout = setTimeout(() => {
        compRef.instance.hide();

        // allow animation to finish (matching CSS transition 240ms)
        setTimeout(() => {
          try {
            this.appRef.detachView(compRef.hostView);
            compRef.destroy();
          } catch {
          }
          resolve();
        }, 260);
      }, duration);

      // optional: if user navigates away / manual clean up later, you can clearTimeout(timeout)
    });
  }
}
