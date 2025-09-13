import {Component, OnDestroy, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {NavigationEnd, Router, RouterOutlet} from '@angular/router';
import {Subject} from 'rxjs';
import {filter, takeUntil} from 'rxjs/operators';

import {ConnectionService} from './core/services/connection.service';
import {Connection} from './core/models/connection.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'PostgreSQL Explorer';
  selectedConnection: Connection | null = null;
  currentRoute = '';

  private destroy$ = new Subject<void>();

  constructor(
    private connectionService: ConnectionService,
    private router: Router
  ) {
  }

  ngOnInit(): void {
    // Subscribe to selected connection changes
    this.connectionService.selectedConnection$
      .pipe(takeUntil(this.destroy$))
      .subscribe(connection => {
        this.selectedConnection = connection;
      });

    // Track current route for navigation highlighting
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe((event) => {
        if (event instanceof NavigationEnd) {
          this.currentRoute = event.url.split('?')[0];
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  navigate(route: string): void {
    this.router.navigate([route]);
  }

  isRouteActive(route: string): boolean {
    return this.currentRoute.startsWith(route);
  }
}
