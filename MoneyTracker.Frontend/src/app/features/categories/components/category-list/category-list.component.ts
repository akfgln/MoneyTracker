import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
    <div class="feature-container">
      <h1>{{ 'CATEGORIES.TITLE' | translate }}</h1>
      <p>Category management coming soon...</p>
    </div>
  `,
  styles: [`
    .feature-container {
      padding: 20px;
    }
  `]
})
export class CategoryListComponent {}