import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ReportsComponent } from './features/reports/reports.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    component: DashboardComponent,
    title: 'Dashboard - Finanzübersicht',
    data: { 
      title: 'Dashboard',
      description: 'Hauptübersicht mit Einnahmen, Ausgaben und Gewinn'
    }
  },
  {
    path: 'reports',
    component: ReportsComponent,
    title: 'Berichte - Finanzberichte',
    data: { 
      title: 'Berichte',
      description: 'Generierung und Verwaltung von Finanzberichten'
    }
  },
  {
    path: 'transactions',
    loadComponent: () => import('./features/transactions/transactions.component')
      .then(c => c.TransactionsComponent),
    title: 'Transaktionen - Buchungsverwaltung',
    data: { 
      title: 'Transaktionen',
      description: 'Verwaltung aller Ein- und Ausgaben'
    }
  },
  {
    path: 'categories',
    loadComponent: () => import('./features/categories/categories.component')
      .then(c => c.CategoriesComponent),
    title: 'Kategorien - Ausgabenkategorien',
    data: { 
      title: 'Kategorien',
      description: 'Verwaltung der Ausgaben- und Einnahmenkategorien'
    }
  },
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings.component')
      .then(c => c.SettingsComponent),
    title: 'Einstellungen - Systemkonfiguration',
    data: { 
      title: 'Einstellungen',
      description: 'System- und Benutzereinstellungen'
    }
  },
  {
    path: 'help',
    loadComponent: () => import('./features/help/help.component')
      .then(c => c.HelpComponent),
    title: 'Hilfe - Unterstützung',
    data: { 
      title: 'Hilfe',
      description: 'Dokumentation und Support'
    }
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];

// Route Guard for authentication (if needed)
export const authGuard = () => {
  // Implement authentication logic here
  return true;
};

// Route resolver for loading common data
export const commonDataResolver = () => {
  return {
    currentYear: new Date().getFullYear(),
    currentMonth: new Date().getMonth() + 1,
    germanLocale: 'de-DE'
  };
};