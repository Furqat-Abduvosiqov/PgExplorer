import {Routes} from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/connections',
    pathMatch: 'full'
  },
  {
    path: 'connections',
    loadComponent: () => import('./features/connections/components/connection-list/connection-list.component').then(c => c.ConnectionListComponent)
  },
  {
    path: 'connections/new',
    loadComponent: () => import('./features/connections/components/connection-form/connection-form.component').then(c => c.ConnectionFormComponent)
  },
  {
    path: 'connections/:id/edit',
    loadComponent: () => import('./features/connections/components/connection-form/connection-form.component').then(c => c.ConnectionFormComponent)
  },
  {
    path: 'databases',
    loadComponent: () => import('./features/databases/components/database-list/database-list.component').then(c => c.DatabaseListComponent)
  },
  {
    path: 'queries',
    loadComponent: () => import('./features/queries/components/query-list/query-list.component').then(c => c.QueryListComponent)
  },
  {
    path: 'query-editor',
    loadComponent: () => import('./features/queries/components/query-editor/query-editor.component').then(c => c.QueryEditorComponent)
  },
  {
    path: 'tables',
    loadComponent: () => import('./features/tables/components/table-list/table-list.component').then(c => c.TableListComponent)
  },
  {
    path: '**',
    redirectTo: '/connections'
  }
];
