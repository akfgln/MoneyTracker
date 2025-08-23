import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Skip auth header for login/register endpoints
  const authEndpoints = ['/auth/login', '/auth/register', '/auth/refresh-token', '/auth/forgot-password', '/auth/reset-password'];
  const isAuthEndpoint = authEndpoints.some(endpoint => req.url.includes(endpoint));
  
  if (isAuthEndpoint) {
    return next(req);
  }

  // Get token from localStorage
  const token = localStorage.getItem('accessToken');
  
  if (token) {
    // Clone request and add authorization header
    const authReq = req.clone({
      setHeaders: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    return next(authReq);
  }
  
  return next(req);
};