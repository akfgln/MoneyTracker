import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

export interface ConfirmationDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'info' | 'warning' | 'danger';
}

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, TranslateModule],
  template: `
    <div class="dialog-container">
      <div class="dialog-header" [ngClass]="data.type || 'info'">
        <mat-icon [ngSwitch]="data.type">
          <ng-container *ngSwitchCase="'warning'">warning</ng-container>
          <ng-container *ngSwitchCase="'danger'">error</ng-container>
          <ng-container *ngSwitchDefault>info</ng-container>
        </mat-icon>
        <h2>{{ data.title | translate }}</h2>
      </div>
      
      <mat-dialog-content class="dialog-content">
        <p>{{ data.message | translate }}</p>
      </mat-dialog-content>
      
      <mat-dialog-actions class="dialog-actions">
        <button mat-button (click)="onCancel()">
          {{ (data.cancelText || 'COMMON.CANCEL') | translate }}
        </button>
        <button 
          mat-raised-button 
          [color]="getConfirmButtonColor()"
          (click)="onConfirm()"
          cdkFocusInitial>
          {{ (data.confirmText || 'COMMON.CONFIRM') | translate }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-container {
      min-width: 300px;
      max-width: 500px;
    }
    
    .dialog-header {
      display: flex;
      align-items: center;
      margin-bottom: 16px;
      
      mat-icon {
        margin-right: 12px;
        font-size: 24px;
        width: 24px;
        height: 24px;
      }
      
      h2 {
        margin: 0;
        font-size: 18px;
        font-weight: 500;
      }
      
      &.info {
        color: #2196f3;
      }
      
      &.warning {
        color: #ff9800;
      }
      
      &.danger {
        color: #f44336;
      }
    }
    
    .dialog-content {
      margin: 0;
      
      p {
        margin: 0;
        line-height: 1.5;
      }
    }
    
    .dialog-actions {
      justify-content: flex-end;
      gap: 8px;
      margin-top: 24px;
      padding: 0;
    }
  `]
})
export class ConfirmationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmationDialogData
  ) {}

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  getConfirmButtonColor(): 'primary' | 'accent' | 'warn' {
    switch (this.data.type) {
      case 'danger':
        return 'warn';
      case 'warning':
        return 'accent';
      default:
        return 'primary';
    }
  }
}