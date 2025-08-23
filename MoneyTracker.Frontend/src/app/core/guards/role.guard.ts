import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { map, take } from 'rxjs/operators';

export interface RoleGuardConfig {
  requiredRoles: string[];
  requireAll?: boolean; // If true, user must have ALL roles. If false, user must have at least ONE role
}

export function createRoleGuard(config: RoleGuardConfig): CanActivateFn {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    
    return authService.currentUser$.pipe(
      take(1),
      map(user => {
        if (!user) {
          router.navigate(['/auth/login'], {
            queryParams: { returnUrl: state.url }
          });
          return false;
        }
        
        const userRoles = user.roles || [];
        const { requiredRoles, requireAll = false } = config;
        
        let hasRequiredRoles: boolean;
        
        if (requireAll) {
          // User must have ALL required roles
          hasRequiredRoles = requiredRoles.every(role => userRoles.includes(role));
        } else {
          // User must have at least ONE required role
          hasRequiredRoles = requiredRoles.some(role => userRoles.includes(role));
        }
        
        if (!hasRequiredRoles) {
          // Redirect to dashboard or access denied page
          router.navigate(['/dashboard']);
          return false;
        }
        
        return true;
      })
    );
  };
}

// Pre-configured role guards
export const AdminGuard = createRoleGuard({ requiredRoles: ['Admin'] });
export const PremiumGuard = createRoleGuard({ requiredRoles: ['Premium', 'Admin'] });
export const UserGuard = createRoleGuard({ requiredRoles: ['User', 'Premium', 'Admin'] });