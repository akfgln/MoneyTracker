import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LoadingService } from '../services/loading.service';
import { finalize } from 'rxjs/operators';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  
  // Skip loading indicator for certain endpoints
  const skipLoadingEndpoints = ['/auth/refresh-token'];
  const skipLoading = skipLoadingEndpoints.some(endpoint => req.url.includes(endpoint));
  
  if (!skipLoading) {
    loadingService.show();
  }
  
  return next(req).pipe(
    finalize(() => {
      if (!skipLoading) {
        loadingService.hide();
      }
    })
  );
};