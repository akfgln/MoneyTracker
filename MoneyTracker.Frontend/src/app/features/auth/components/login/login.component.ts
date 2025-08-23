import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../../../core/services/auth.service';
import { LoadingService } from '../../../../core/services/loading.service';
import { ErrorMessageComponent } from '../../../../shared/components/error-message/error-message.component';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';
import { LoginRequest } from '../../../../core/models/user.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatIconModule,
    MatSnackBarModule,
    TranslateModule,
    ErrorMessageComponent,
    LoadingSpinnerComponent
  ],
  template: `
    <div class="login-container">
      <mat-card class="login-card">
        <mat-card-header>
          <div class="login-header">
            <h1>{{ 'AUTH.LOGIN' | translate }}</h1>
            <p class="subtitle">{{ 'APP.SUBTITLE' | translate }}</p>
          </div>
        </mat-card-header>
        
        <mat-card-content>
          <app-error-message 
            *ngIf="errorMessage"
            [message]="errorMessage"
            [type]="'error'"
            [dismissible]="true"
            (dismiss)="clearError()">
          </app-error-message>
          
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="german-form">
            <mat-form-field appearance="outline">
              <mat-label>{{ 'AUTH.EMAIL' | translate }}</mat-label>
              <input 
                matInput 
                type="email" 
                formControlName="email"
                [placeholder]="'AUTH.EMAIL' | translate"
                autocomplete="email">
              <mat-icon matSuffix>email</mat-icon>
              <mat-error *ngIf="loginForm.get('email')?.hasError('required')">
                {{ 'COMMON.REQUIRED_FIELD' | translate }}
              </mat-error>
              <mat-error *ngIf="loginForm.get('email')?.hasError('email')">
                {{ 'COMMON.INVALID_EMAIL' | translate }}
              </mat-error>
            </mat-form-field>
            
            <mat-form-field appearance="outline">
              <mat-label>{{ 'AUTH.PASSWORD' | translate }}</mat-label>
              <input 
                matInput 
                [type]="hidePassword ? 'password' : 'text'"
                formControlName="password"
                [placeholder]="'AUTH.PASSWORD' | translate"
                autocomplete="current-password">
              <button 
                mat-icon-button 
                matSuffix 
                type="button"
                (click)="hidePassword = !hidePassword"
                [attr.aria-label]="'Show password'"
                [attr.aria-pressed]="!hidePassword">
                <mat-icon>{{hidePassword ? 'visibility' : 'visibility_off'}}</mat-icon>
              </button>
              <mat-error *ngIf="loginForm.get('password')?.hasError('required')">
                {{ 'COMMON.REQUIRED_FIELD' | translate }}
              </mat-error>
            </mat-form-field>
            
            <div class="form-options">
              <mat-checkbox formControlName="rememberMe">
                {{ 'AUTH.REMEMBER_ME' | translate }}
              </mat-checkbox>
              
              <a routerLink="/auth/forgot-password" class="forgot-password-link">
                {{ 'AUTH.FORGOT_PASSWORD' | translate }}
              </a>
            </div>
            
            <button 
              mat-raised-button 
              color="primary" 
              type="submit"
              class="login-button"
              [disabled]="loginForm.invalid || isLoading">
              <app-loading-spinner 
                *ngIf="isLoading" 
                [showText]="false"
                style="display: inline-block; margin-right: 8px;">
              </app-loading-spinner>
              {{ 'AUTH.LOGIN' | translate }}
            </button>
          </form>
          
          <div class="register-link">
            <span>{{ 'AUTH.NO_ACCOUNT' | translate }}</span>
            <a routerLink="/auth/register" class="register-link-button">
              {{ 'AUTH.REGISTER' | translate }}
            </a>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 20px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }
    
    .login-card {
      width: 100%;
      max-width: 400px;
      padding: 0;
    }
    
    .login-header {
      text-align: center;
      width: 100%;
      
      h1 {
        margin: 0 0 8px 0;
        color: #333;
        font-weight: 500;
      }
      
      .subtitle {
        margin: 0;
        color: #666;
        font-size: 14px;
      }
    }
    
    .german-form {
      width: 100%;
      
      .mat-mdc-form-field {
        width: 100%;
        margin-bottom: 16px;
      }
    }
    
    .form-options {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin: 16px 0 24px 0;
      
      .forgot-password-link {
        color: #1976d2;
        text-decoration: none;
        font-size: 14px;
        
        &:hover {
          text-decoration: underline;
        }
      }
    }
    
    .login-button {
      width: 100%;
      height: 48px;
      font-size: 16px;
      font-weight: 500;
    }
    
    .register-link {
      text-align: center;
      margin-top: 24px;
      padding-top: 16px;
      border-top: 1px solid #e0e0e0;
      
      span {
        color: #666;
        margin-right: 8px;
      }
      
      .register-link-button {
        color: #1976d2;
        text-decoration: none;
        font-weight: 500;
        
        &:hover {
          text-decoration: underline;
        }
      }
    }
    
    @media (max-width: 480px) {
      .login-container {
        padding: 12px;
      }
      
      .form-options {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;
      }
    }
  `]
})
export class LoginComponent implements OnInit, OnDestroy {
  loginForm!: FormGroup;
  hidePassword = true;
  isLoading = false;
  errorMessage: string | null = null;
  private destroy$ = new Subject<void>();
  private returnUrl = '/dashboard';

  constructor(
    private authService: AuthService,
    private formBuilder: FormBuilder,
    private loadingService: LoadingService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.createForm();
    this.setupLoadingSubscription();
    
    // Get return URL from route parameters or use default
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
    
    // If user is already logged in, redirect
    if (this.authService.isAuthenticated()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): void {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  private setupLoadingSubscription(): void {
    this.loadingService.loading$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loading => {
        this.isLoading = loading;
      });
  }

  onSubmit(): void {
    if (this.loginForm.valid && !this.isLoading) {
      this.clearError();
      
      const loginData: LoginRequest = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password,
        rememberMe: this.loginForm.value.rememberMe
      };
      
      this.authService.login(loginData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.snackBar.open(
                this.translate.instant('AUTH.LOGIN_SUCCESS'), 
                this.translate.instant('COMMON.CLOSE'),
                { duration: 3000, panelClass: ['success-snackbar'] }
              );
              this.router.navigate([this.returnUrl]);
            } else {
              this.errorMessage = response.message || this.translate.instant('AUTH.INVALID_CREDENTIALS');
            }
          },
          error: (error) => {
            console.error('Login error:', error);
            this.errorMessage = error.error?.message || this.translate.instant('AUTH.INVALID_CREDENTIALS');
          }
        });
    } else {
      this.markFormGroupTouched();
    }
  }

  clearError(): void {
    this.errorMessage = null;
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      if (control) {
        control.markAsTouched();
      }
    });
  }
}