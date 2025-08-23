import { Routes } from '@angular/router';

export const categoryRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./components/category-list/category-list.component').then(m => m.CategoryListComponent)
  }
];