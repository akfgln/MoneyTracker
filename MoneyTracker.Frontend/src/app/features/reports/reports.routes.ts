import { Routes } from '@angular/router';

export const reportRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/reports-overview/reports-overview.component').then(m => m.ReportsOverviewComponent)
  }
];