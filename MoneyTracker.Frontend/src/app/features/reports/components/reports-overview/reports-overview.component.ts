import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-reports-overview',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="feature-container">
      <h1>{{ 'REPORTS.TITLE' | translate }}</h1>
      <p>Reports and analytics coming soon...</p>
    </div>
  `,
  styles: [`
    .feature-container {
      padding: 20px;
    }
  `]
})
export class ReportsOverviewComponent {}