import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="feature-container">
      <h1>{{ 'SETTINGS.TITLE' | translate }}</h1>
      <p>Settings and preferences coming soon...</p>
    </div>
  `,
  styles: [`
    .feature-container {
      padding: 20px;
    }
  `]
})
export class SettingsComponent {}