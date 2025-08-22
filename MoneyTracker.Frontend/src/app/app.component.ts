import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TranslateModule } from '@ngx-translate/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  template: `
    <mat-sidenav-container class="sidenav-container">
      <mat-sidenav #drawer class="sidenav" fixedInViewport
          [attr.role]="(isHandset$ | async) ? 'dialog' : 'navigation'"
          [mode]="(isHandset$ | async) ? 'over' : 'side'"
          [opened]="(isHandset$ | async) === false">
        <mat-toolbar class="sidenav-header">
          <mat-icon class="logo-icon">account_balance</mat-icon>
          <span class="app-name">{{ 'app.title' | translate }}</span>
        </mat-toolbar>
        
        <mat-nav-list>
          <a mat-list-item routerLink="/dashboard" routerLinkActive="active-link">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <span matListItemTitle>{{ 'navigation.dashboard' | translate }}</span>
          </a>
          
          <a mat-list-item routerLink="/reports" routerLinkActive="active-link">
            <mat-icon matListItemIcon>assessment</mat-icon>
            <span matListItemTitle>{{ 'navigation.reports' | translate }}</span>
          </a>
          
          <a mat-list-item routerLink="/transactions" routerLinkActive="active-link">
            <mat-icon matListItemIcon>receipt_long</mat-icon>
            <span matListItemTitle>{{ 'navigation.transactions' | translate }}</span>
          </a>
          
          <a mat-list-item routerLink="/categories" routerLinkActive="active-link">
            <mat-icon matListItemIcon>category</mat-icon>
            <span matListItemTitle>{{ 'navigation.categories' | translate }}</span>
          </a>
          
          <mat-divider></mat-divider>
          
          <a mat-list-item routerLink="/settings" routerLinkActive="active-link">
            <mat-icon matListItemIcon>settings</mat-icon>
            <span matListItemTitle>{{ 'navigation.settings' | translate }}</span>
          </a>
          
          <a mat-list-item routerLink="/help" routerLinkActive="active-link">
            <mat-icon matListItemIcon>help</mat-icon>
            <span matListItemTitle>{{ 'navigation.help' | translate }}</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>
      
      <mat-sidenav-content>
        <mat-toolbar color="primary" class="main-toolbar">
          <button
            type="button"
            aria-label="Toggle sidenav"
            mat-icon-button
            (click)="drawer.toggle()"
            *ngIf="isHandset$ | async">
            <mat-icon aria-label="Side nav toggle icon">menu</mat-icon>
          </button>
          
          <span class="toolbar-title">{{ getPageTitle() }}</span>
          
          <span class="toolbar-spacer"></span>
          
          <button mat-icon-button [matMenuTriggerFor]="userMenu">
            <mat-icon>account_circle</mat-icon>
          </button>
          
          <mat-menu #userMenu="matMenu">
            <button mat-menu-item>
              <mat-icon>person</mat-icon>
              <span>{{ 'user.profile' | translate }}</span>
            </button>
            <button mat-menu-item>
              <mat-icon>logout</mat-icon>
              <span>{{ 'user.logout' | translate }}</span>
            </button>
          </mat-menu>
        </mat-toolbar>
        
        <main class="main-content">
          <router-outlet></router-outlet>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .sidenav-container {
      height: 100vh;
    }
    
    .sidenav {
      width: 280px;
      background-color: #fafafa;
      border-right: 1px solid #e0e0e0;
    }
    
    .sidenav-header {
      background-color: #1976d2;
      color: white;
      display: flex;
      align-items: center;
      padding: 0 16px;
      min-height: 64px;
    }
    
    .logo-icon {
      margin-right: 12px;
      font-size: 28px;
      width: 28px;
      height: 28px;
    }
    
    .app-name {
      font-weight: 500;
      font-size: 18px;
    }
    
    .main-toolbar {
      position: sticky;
      top: 0;
      z-index: 1;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    
    .toolbar-title {
      font-weight: 500;
      font-size: 18px;
    }
    
    .toolbar-spacer {
      flex: 1 1 auto;
    }
    
    .main-content {
      height: calc(100vh - 64px);
      overflow-y: auto;
    }
    
    .active-link {
      background-color: rgba(25, 118, 210, 0.1) !important;
      color: #1976d2 !important;
    }
    
    .active-link mat-icon {
      color: #1976d2 !important;
    }
    
    mat-nav-list a {
      margin: 4px 8px;
      border-radius: 8px;
      transition: background-color 0.2s ease;
    }
    
    mat-nav-list a:hover {
      background-color: rgba(0, 0, 0, 0.04);
    }
    
    @media (max-width: 768px) {
      .sidenav {
        width: 100vw;
      }
      
      .main-content {
        height: calc(100vh - 56px);
      }
      
      .sidenav-header {
        min-height: 56px;
      }
    }
  `],
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    TranslateModule
  ]
})
export class AppComponent {
  title = 'german-financial-dashboard';
  
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );
    
  constructor(private breakpointObserver: BreakpointObserver) {}
  
  getPageTitle(): string {
    // This would typically be determined by the current route
    // For now, return a default title
    return 'Finanzdashboard';
  }
}