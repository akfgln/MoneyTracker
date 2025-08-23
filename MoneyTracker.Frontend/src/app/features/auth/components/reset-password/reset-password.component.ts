import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="auth-container">
      <h1>Reset Password</h1>
      <p>Reset password component coming soon...</p>
    </div>
  `,
  styles: [`
    .auth-container {
      padding: 20px;
      text-align: center;
    }
  `]
})
export class ResetPasswordComponent {}