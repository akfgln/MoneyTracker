import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="auth-container">
      <h1>{{ 'AUTH.REGISTER' | translate }}</h1>
      <p>Registration component coming soon...</p>
    </div>
  `,
  styles: [`
    .auth-container {
      padding: 20px;
      text-align: center;
    }
  `]
})
export class RegisterComponent {}