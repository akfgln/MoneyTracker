import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule, MatSidenav } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';

import { AuthService } from './core/services/auth.service';
import { User } from './core/models/user.model';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    TranslateModule
  ],
  template: `
    <div class="app-container">
      <mat-sidenav-container class="sidenav-container">
        <mat-sidenav
          #drawer
          class="sidenav"
          fixedInViewport
          [attr.role]="(isHandset$ | async) ? 'dialog' : 'navigation'"
          [mode]="(isHandset$ | async) ? 'over' : 'side'"
          [opened]="(isHandset$ | async) === false && isAuthenticated">
          
          <mat-toolbar>{{ 'APP.TITLE' | translate }}</mat-toolbar>
          
          <mat-nav-list *ngIf="isAuthenticated">
            <a mat-list-item routerLink="/dashboard" routerLinkActive="active">
              <mat-icon matListItemIcon>dashboard</mat-icon>
              <span matListItemTitle>{{ 'NAVIGATION.DASHBOARD' | translate }}</span>
            </a>
            
            <a mat-list-item routerLink="/transactions" routerLinkActive="active">
              <mat-icon matListItemIcon>account_balance_wallet</mat-icon>
              <span matListItemTitle>{{ 'NAVIGATION.TRANSACTIONS' | translate }}</span>
            </a>
            
            <a mat-list-item routerLink="/categories" routerLinkActive="active">
              <mat-icon matListItemIcon>category</mat-icon>
              <span matListItemTitle>{{ 'NAVIGATION.CATEGORIES' | translate }}</span>
            </a>
            
            <a mat-list-item routerLink="/reports" routerLinkActive="active">
              <mat-icon matListItemIcon>assessment</mat-icon>
              <span matListItemTitle>{{ 'NAVIGATION.REPORTS' | translate }}</span>
            </a>
            
            <a mat-list-item routerLink="/settings" routerLinkActive="active">
              <mat-icon matListItemIcon>settings</mat-icon>
              <span matListItemTitle>{{ 'NAVIGATION.SETTINGS' | translate }}</span>
            </a>
          </mat-nav-list>
          
          <div class="sidenav-footer">
            <mat-nav-list>
              <a mat-list-item href="#" (click)="$event.preventDefault()">
                <mat-icon matListItemIcon>help</mat-icon>
                <span matListItemTitle>{{ 'NAVIGATION.HELP' | translate }}</span>
              </a>
            </mat-nav-list>
            
            <div class="version-info">
              {{ 'APP.VERSION' | translate: {version: appVersion} }}
            </div>
          </div>
        </mat-sidenav>
        
        <mat-sidenav-content>
          <mat-toolbar color="primary" class="main-toolbar">
            <button
              type="button"
              aria-label="Toggle sidenav"
              mat-icon-button
              (click)="drawer.toggle()"
              *ngIf="isAuthenticated">
              <mat-icon>menu</mat-icon>
            </button>
            
            <span class="toolbar-title">{{ 'APP.SUBTITLE' | translate }}</span>
            
            <span class="spacer"></span>
            
            <div *ngIf="isAuthenticated" class="user-menu">
              <button mat-button [matMenuTriggerFor]="userMenu">
                <mat-icon>account_circle</mat-icon>
                <span class="username">{{ currentUser?.firstName }} {{ currentUser?.lastName }}</span>
                <mat-icon>arrow_drop_down</mat-icon>
              </button>
              
              <mat-menu #userMenu="matMenu">
                <a mat-menu-item routerLink="/settings">
                  <mat-icon>person</mat-icon>
                  <span>{{ 'NAVIGATION.PROFILE' | translate }}</span>
                </a>
                
                <mat-divider></mat-divider>
                
                <button mat-menu-item (click)="logout()">
                  <mat-icon>exit_to_app</mat-icon>
                  <span>{{ 'AUTH.LOGOUT' | translate }}</span>
                </button>
              </mat-menu>
            </div>
            
            <div *ngIf="!isAuthenticated" class="auth-buttons">
              <a mat-button routerLink="/auth/login">{{ 'AUTH.LOGIN' | translate }}</a>
              <a mat-raised-button color="accent" routerLink="/auth/register">{{ 'AUTH.REGISTER' | translate }}</a>
            </div>
          </mat-toolbar>
          
          <div class="content-container">
            <router-outlet></router-outlet>
          </div>
        </mat-sidenav-content>
      </mat-sidenav-container>
    </div>
  `,
  styles: [`
    .app-container {
      height: 100vh;
      display: flex;
      flex-direction: column;
    }
    
    .sidenav-container {
      flex: 1;
    }
    
    .sidenav {
      width: 280px;
      background: #fafafa;
      border-right: 1px solid #e0e0e0;
    }
    
    .sidenav .mat-toolbar {
      background: #37474f;
      color: white;
      font-weight: 500;
      justify-content: center;
    }
    
    .sidenav-footer {
      position: absolute;
      bottom: 0;
      width: 100%;
      border-top: 1px solid #e0e0e0;
      background: white;
    }
    
    .version-info {
      text-align: center;
      padding: 8px;
      font-size: 12px;
      color: #666;
    }
    
    .main-toolbar {
      position: sticky;
      top: 0;
      z-index: 1000;
    }
    
    .toolbar-title {
      font-size: 18px;
      font-weight: 500;
    }
    
    .spacer {
      flex: 1 1 auto;
    }
    
    .user-menu .username {
      margin: 0 8px;
      font-weight: 500;
    }
    
    .auth-buttons {
      display: flex;
      gap: 8px;
    }
    
    .content-container {
      height: calc(100vh - 64px);
      overflow: auto;
    }
    
    .mat-list-item.active {
      background-color: rgba(63, 81, 181, 0.1);
      color: #3f51b5;
      
      .mat-icon {
        color: #3f51b5;
      }
    }
    
    @media (max-width: 768px) {
      .content-container {
        padding: 16px 12px;
      }
      
      .user-menu .username {
        display: none;
      }
    }
  `]
})
export class AppComponent implements OnInit {
  @ViewChild('drawer') drawer!: MatSidenav;
  
  isAuthenticated = false;
  currentUser: User | null = null;
  appVersion = environment.appVersion;
  
  isHandset$: Observable<boolean> = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  constructor(
    private breakpointObserver: BreakpointObserver,
    private authService: AuthService,
    private router: Router,
    private translate: TranslateService
  ) {
    // Set up German as default language
    this.translate.setDefaultLang('de');
    this.translate.use('de');
  }

  ngOnInit(): void {
    // Subscribe to authentication status
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      this.isAuthenticated = !!user;
      
      // If user is not authenticated, navigate to login
      if (!this.isAuthenticated && !this.router.url.startsWith('/auth')) {
        this.router.navigate(['/auth/login']);
      }
    });
  }
  
  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      },
      error: (error) => {
        console.error('Logout error:', error);
        // Still navigate to login even if logout request fails
        this.router.navigate(['/auth/login']);
      }
    });
  }
}