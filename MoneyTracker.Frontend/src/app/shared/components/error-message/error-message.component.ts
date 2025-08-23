import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TranslateModule } from '@ngx-translate/core';

export type MessageType = 'error' | 'warning' | 'info' | 'success';

@Component({
  selector: 'app-error-message',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, TranslateModule],
  template: `
    <div class="message-container" [ngClass]="type" *ngIf="message">
      <div class="message-content">
        <mat-icon class="message-icon" [ngSwitch]="type">
          <ng-container *ngSwitchCase="'error'">error</ng-container>
          <ng-container *ngSwitchCase="'warning'">warning</ng-container>
          <ng-container *ngSwitchCase="'success'">check_circle</ng-container>
          <ng-container *ngSwitchDefault>info</ng-container>
        </mat-icon>
        
        <div class="message-text">
          <div class="message-title" *ngIf="title">{{ title | translate }}</div>
          <div class="message-body">{{ message | translate }}</div>
        </div>
        
        <button 
          *ngIf="dismissible" 
          mat-icon-button 
          class="dismiss-button"
          (click)="onDismiss()"
          [attr.aria-label]="'COMMON.CLOSE' | translate">
          <mat-icon>close</mat-icon>
        </button>
      </div>
      
      <div class="message-actions" *ngIf="showRetry">
        <button mat-button (click)="onRetry()">
          <mat-icon>refresh</mat-icon>
          {{ 'COMMON.RETRY' | translate }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .message-container {
      border-radius: 4px;
      border: 1px solid;
      margin: 8px 0;
      overflow: hidden;
      
      &.error {
        background-color: #ffebee;
        border-color: #f44336;
        color: #c62828;
      }
      
      &.warning {
        background-color: #fff8e1;
        border-color: #ff9800;
        color: #ef6c00;
      }
      
      &.info {
        background-color: #e3f2fd;
        border-color: #2196f3;
        color: #1565c0;
      }
      
      &.success {
        background-color: #e8f5e8;
        border-color: #4caf50;
        color: #2e7d32;
      }
    }
    
    .message-content {
      display: flex;
      align-items: flex-start;
      padding: 12px 16px;
      gap: 12px;
    }
    
    .message-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
      flex-shrink: 0;
      margin-top: 2px;
    }
    
    .message-text {
      flex: 1;
      min-width: 0;
    }
    
    .message-title {
      font-weight: 600;
      margin-bottom: 4px;
      font-size: 14px;
    }
    
    .message-body {
      font-size: 14px;
      line-height: 1.4;
      word-wrap: break-word;
    }
    
    .dismiss-button {
      flex-shrink: 0;
      width: 32px;
      height: 32px;
      
      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
      }
    }
    
    .message-actions {
      border-top: 1px solid currentColor;
      border-top-opacity: 0.2;
      padding: 8px 16px;
      
      button {
        font-size: 12px;
        height: 32px;
        
        mat-icon {
          font-size: 16px;
          width: 16px;
          height: 16px;
          margin-right: 4px;
        }
      }
    }
  `]
})
export class ErrorMessageComponent {
  @Input() type: MessageType = 'error';
  @Input() title?: string;
  @Input() message?: string;
  @Input() dismissible: boolean = true;
  @Input() showRetry: boolean = false;
  
  @Output() dismiss = new EventEmitter<void>();
  @Output() retry = new EventEmitter<void>();

  onDismiss(): void {
    this.dismiss.emit();
  }

  onRetry(): void {
    this.retry.emit();
  }
}