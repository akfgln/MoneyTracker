import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule, TranslateModule],
  template: `
    <div class="loading-container">
      <mat-spinner [diameter]="diameter" [color]="color"></mat-spinner>
      <p class="loading-text" *ngIf="showText">{{ text | translate }}</p>
    </div>
  `,
  styles: [`
    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 20px;
    }
    
    .loading-text {
      margin-top: 16px;
      color: #666;
      font-size: 14px;
      text-align: center;
    }
  `]
})
export class LoadingSpinnerComponent {
  @Input() showText: boolean = true;
  @Input() text: string = 'COMMON.LOADING';
  @Input() color: 'primary' | 'accent' | 'warn' = 'primary';
  @Input() diameter: number = 50;
}