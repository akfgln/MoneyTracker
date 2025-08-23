import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);
  const translate = inject(TranslateService);
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = '';
      
      switch (error.status) {
        case 0:
          // Network error
          errorMessage = translate.instant('ERRORS.NETWORK_ERROR');
          break;
          
        case 401:
          // Unauthorized - redirect to login unless already on auth pages
          if (!router.url.startsWith('/auth')) {
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            router.navigate(['/auth/login']);
            errorMessage = translate.instant('ERRORS.UNAUTHORIZED');
          }
          break;
          
        case 403:
          // Forbidden
          errorMessage = translate.instant('ERRORS.FORBIDDEN');
          break;
          
        case 404:
          // Not Found
          errorMessage = translate.instant('ERRORS.NOT_FOUND');
          break;
          
        case 422:
          // Validation Error
          if (error.error && error.error.errors) {
            errorMessage = Array.isArray(error.error.errors) 
              ? error.error.errors.join(', ')
              : error.error.message || translate.instant('ERRORS.VALIDATION_ERROR');
          } else {
            errorMessage = translate.instant('ERRORS.VALIDATION_ERROR');
          }
          break;
          
        case 500:
        case 502:
        case 503:
        case 504:
          // Server errors
          errorMessage = translate.instant('ERRORS.SERVER_ERROR');
          break;
          
        default:
          // Unknown error
          errorMessage = error.error?.message || translate.instant('ERRORS.UNKNOWN_ERROR');
          break;
      }
      
      // Show error message to user (except for 401 on auth pages)
      if (!(error.status === 401 && router.url.startsWith('/auth'))) {
        snackBar.open(errorMessage, translate.instant('COMMON.CLOSE'), {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
      
      console.error('HTTP Error:', error);
      
      return throwError(() => error);
    })
  );
};