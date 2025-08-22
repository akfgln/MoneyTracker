import { Routes } from '@angular/router';

export const transactionRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/transaction-management.component')
      .then(m => m.TransactionManagementComponent),
    title: 'Transaktionsverwaltung'
  },
  {
    path: 'list',
    loadComponent: () => import('./components/transaction-list/transaction-list.component')
      .then(m => m.TransactionListComponent),
    title: 'Transaktionen - Übersicht'
  },
  {
    path: 'create',
    loadComponent: () => import('./components/transaction-form/transaction-form.component')
      .then(m => m.TransactionFormComponent),
    title: 'Neue Transaktion'
  },
  {
    path: 'edit/:id',
    loadComponent: () => import('./components/transaction-form/transaction-form.component')
      .then(m => m.TransactionFormComponent),
    title: 'Transaktion bearbeiten'
  },
  {
    path: 'upload',
    loadComponent: () => import('./components/pdf-upload/pdf-upload.component')
      .then(m => m.PdfUploadComponent),
    title: 'Dokumente hochladen'
  },
  {
    path: 'categories',
    loadComponent: () => import('./components/category-select/category-select.component')
      .then(m => m.CategorySelectComponent),
    title: 'Kategorien verwalten'
  }
];