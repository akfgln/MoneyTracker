import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError, finalize } from 'rxjs/operators';

import { User, AuthResponse, LoginRequest, RegisterRequest } from '../models/user.model';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private refreshTokenTimeout: any;

  constructor(private http: HttpClient) {
    // Check for existing token on service initialization
    const token = this.getAccessToken();
    if (token && !this.isTokenExpired(token)) {
      this.loadUserProfile();
    } else {
      this.clearTokens();
    }
  }

  login(loginData: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/auth/login`, loginData)
      .pipe(
        tap(response => {
          if (response.success && response.data) {
            this.setTokens(response.data.accessToken, response.data.refreshToken);
            this.currentUserSubject.next(response.data.user);
            this.startRefreshTokenTimer();
          }
        }),
        catchError(this.handleError)
      );
  }

  register(registerData: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/auth/register`, registerData)
      .pipe(
        tap(response => {
          if (response.success && response.data) {
            this.setTokens(response.data.accessToken, response.data.refreshToken);
            this.currentUserSubject.next(response.data.user);
            this.startRefreshTokenTimer();
          }
        }),
        catchError(this.handleError)
      );
  }

  logout(): Observable<ApiResponse<void>> {
    const refreshToken = this.getRefreshToken();
    
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/auth/logout`, { refreshToken })
      .pipe(
        finalize(() => {
          this.clearTokens();
          this.currentUserSubject.next(null);
          this.stopRefreshTokenTimer();
        }),
        catchError(error => {
          // Even if logout fails, clear local tokens
          this.clearTokens();
          this.currentUserSubject.next(null);
          this.stopRefreshTokenTimer();
          return this.handleError(error);
        })
      );
  }

  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const refreshToken = this.getRefreshToken();
    
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/auth/refresh-token`, { refreshToken })
      .pipe(
        tap(response => {
          if (response.success && response.data) {
            this.setTokens(response.data.accessToken, response.data.refreshToken);
            this.currentUserSubject.next(response.data.user);
            this.startRefreshTokenTimer();
          }
        }),
        catchError(error => {
          // If refresh fails, logout the user
          this.logout();
          return this.handleError(error);
        })
      );
  }

  forgotPassword(email: string): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/auth/forgot-password`, { email })
      .pipe(catchError(this.handleError));
  }

  resetPassword(token: string, newPassword: string): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/auth/reset-password`, {
      token,
      newPassword
    }).pipe(catchError(this.handleError));
  }

  changePassword(currentPassword: string, newPassword: string): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/auth/change-password`, {
      currentPassword,
      newPassword
    }).pipe(catchError(this.handleError));
  }

  confirmEmail(token: string): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/auth/confirm-email`, { token })
      .pipe(catchError(this.handleError));
  }

  resendEmailConfirmation(): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/auth/resend-email-confirmation`, {})
      .pipe(catchError(this.handleError));
  }

  // Token management methods
  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    return token != null && !this.isTokenExpired(token);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  private setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }

  private clearTokens(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp < currentTime;
    } catch {
      return true;
    }
  }

  private loadUserProfile(): void {
    this.http.get<ApiResponse<User>>(`${this.apiUrl}/auth/profile`)
      .pipe(catchError(this.handleError))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.currentUserSubject.next(response.data);
            this.startRefreshTokenTimer();
          }
        },
        error: () => {
          this.clearTokens();
        }
      });
  }

  private startRefreshTokenTimer(): void {
    const token = this.getAccessToken();
    if (!token) return;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expires = new Date(payload.exp * 1000);
      const timeout = expires.getTime() - Date.now() - (5 * 60 * 1000); // Refresh 5 minutes before expiry
      
      if (timeout > 0) {
        this.refreshTokenTimeout = setTimeout(() => {
          this.refreshToken().subscribe();
        }, timeout);
      }
    } catch {
      // If token parsing fails, clear tokens
      this.clearTokens();
    }
  }

  private stopRefreshTokenTimer(): void {
    if (this.refreshTokenTimeout) {
      clearTimeout(this.refreshTokenTimeout);
    }
  }

  private handleError = (error: any): Observable<never> => {
    console.error('Auth Service Error:', error);
    return throwError(() => error);
  };
}