import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { TransactionService } from '../../../../core/services/transaction.service';
import {
  Transaction,
  Category,
  BulkUpdateTransactionsDto,
  TransactionType
} from '../../../../core/models/transaction.model';

export interface BulkUpdateDialogData {
  transactions: Transaction[];
  categories: Category[];
}

@Component({
  selector: 'app-bulk-update-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    TranslateModule
  ],
  template: `
    <form [formGroup]="bulkUpdateForm" (ngSubmit)="onSubmit()">
      <h2 mat-dialog-title>
        <mat-icon>edit</mat-icon>
        {{ 'TRANSACTIONS.BULK_UPDATE' | translate }}
      </h2>
      
      <mat-dialog-content class="bulk-update-content">
        <!-- Selected Transactions Summary -->
        <div class="selected-summary">
          <h4>{{ 'TRANSACTIONS.SELECTED_TRANSACTIONS' | translate }}</h4>
          <div class="summary-stats">
            <div class="stat-item">
              <mat-icon>format_list_numbered</mat-icon>
              <span>{{ selectedTransactions.length }} {{ 'TRANSACTIONS.ITEMS' | translate }}</span>
            </div>
            <div class="stat-item">
              <mat-icon class="income-icon">trending_up</mat-icon>
              <span>{{ getIncomeCount() }} {{ 'TRANSACTIONS.INCOME' | translate }}</span>
            </div>
            <div class="stat-item">
              <mat-icon class="expense-icon">trending_down</mat-icon>
              <span>{{ getExpenseCount() }} {{ 'TRANSACTIONS.EXPENSE' | translate }}</span>
            </div>
          </div>
          
          <!-- Transaction Preview -->
          <div class="transaction-preview">
            <mat-chip-listbox>
              <mat-chip *ngFor="let transaction of previewTransactions" 
                       [class.income-chip]="transaction.transactionType === 'Income'"
                       [class.expense-chip]="transaction.transactionType === 'Expense'">
                <mat-icon matChipAvatar>
                  {{ transaction.transactionType === 'Income' ? 'trending_up' : 'trending_down' }}
                </mat-icon>
                <span class="transaction-desc">{{ transaction.description }}</span>
                <span class="transaction-amount">
                  {{ (transaction.transactionType === 'Income' ? '+' : '-') }}
                  {{ transaction.amount | currency:'EUR':'symbol':'1.2-2':'de' }}
                </span>
              </mat-chip>
            </mat-chip-listbox>
            
            <div class="more-indicator" *ngIf="selectedTransactions.length > 3">
              <small>{{ 'TRANSACTIONS.AND_MORE' | translate: { count: selectedTransactions.length - 3 } }}</small>
            </div>
          </div>
        </div>
        
        <!-- Update Options -->
        <div class="update-options">
          <h4>{{ 'TRANSACTIONS.UPDATE_FIELDS' | translate }}</h4>
          <p class="update-hint">{{ 'TRANSACTIONS.BULK_UPDATE_HINT' | translate }}</p>
          
          <!-- Category Update -->
          <div class="update-field">
            <mat-checkbox formControlName="updateCategory" (change)="onFieldToggle('category')">
              {{ 'TRANSACTIONS.UPDATE_CATEGORY' | translate }}
            </mat-checkbox>
            
            <mat-form-field appearance="outline" 
                           class="field-input" 
                           *ngIf="bulkUpdateForm.get('updateCategory')?.value">
              <mat-label>{{ 'TRANSACTIONS.NEW_CATEGORY' | translate }}</mat-label>
              <mat-select formControlName="categoryId">
                <mat-optgroup *ngFor="let group of getCategoryGroups()" [label]="group.label">
                  <mat-option *ngFor="let category of group.categories" [value]="category.id">
                    <div class="category-option">
                      <mat-icon [style.color]="category.color">{{ category.icon }}</mat-icon>
                      <span>{{ category.name }}</span>
                    </div>
                  </mat-option>
                </mat-optgroup>
              </mat-select>
            </mat-form-field>
          </div>
          
          <!-- Reconciliation Status -->
          <div class="update-field">
            <mat-checkbox formControlName="updateReconciled" (change)="onFieldToggle('reconciled')">
              {{ 'TRANSACTIONS.UPDATE_RECONCILIATION' | translate }}
            </mat-checkbox>
            
            <mat-form-field appearance="outline" 
                           class="field-input" 
                           *ngIf="bulkUpdateForm.get('updateReconciled')?.value">
              <mat-label>{{ 'TRANSACTIONS.RECONCILIATION_STATUS' | translate }}</mat-label>
              <mat-select formControlName="isReconciled">
                <mat-option [value]="true">
                  <mat-icon color="primary">check_circle</mat-icon>
                  {{ 'TRANSACTIONS.RECONCILED' | translate }}
                </mat-option>
                <mat-option [value]="false">
                  <mat-icon>radio_button_unchecked</mat-icon>
                  {{ 'TRANSACTIONS.NOT_RECONCILED' | translate }}
                </mat-option>
              </mat-select>
            </mat-form-field>
          </div>
          
          <!-- Account Update -->
          <div class="update-field">
            <mat-checkbox formControlName="updateAccount" (change)="onFieldToggle('account')">
              {{ 'TRANSACTIONS.UPDATE_ACCOUNT' | translate }}
            </mat-checkbox>
            
            <mat-form-field appearance="outline" 
                           class="field-input" 
                           *ngIf="bulkUpdateForm.get('updateAccount')?.value">
              <mat-label>{{ 'TRANSACTIONS.NEW_ACCOUNT' | translate }}</mat-label>
              <mat-select formControlName="accountId">
                <mat-option *ngFor="let account of accounts" [value]="account.id">
                  <div class="account-option">
                    <mat-icon>account_balance</mat-icon>
                    <div class="account-info">
                      <span class="account-name">{{ account.accountName }}</span>
                      <small class="account-iban">{{ account.iban }}</small>
                    </div>
                  </div>
                </mat-option>
              </mat-select>
            </mat-form-field>
          </div>
          
          <!-- Transaction Type (only if all selected are same type) -->
          <div class="update-field" *ngIf="canUpdateTransactionType()">
            <mat-checkbox formControlName="updateTransactionType" (change)="onFieldToggle('transactionType')">
              {{ 'TRANSACTIONS.UPDATE_TYPE' | translate }}
            </mat-checkbox>
            
            <mat-form-field appearance="outline" 
                           class="field-input" 
                           *ngIf="bulkUpdateForm.get('updateTransactionType')?.value">
              <mat-label>{{ 'TRANSACTIONS.NEW_TYPE' | translate }}</mat-label>
              <mat-select formControlName="transactionType">
                <mat-option value="Income">
                  <mat-icon class="income-icon">trending_up</mat-icon>
                  {{ 'TRANSACTIONS.INCOME' | translate }}
                </mat-option>
                <mat-option value="Expense">
                  <mat-icon class="expense-icon">trending_down</mat-icon>
                  {{ 'TRANSACTIONS.EXPENSE' | translate }}
                </mat-option>
              </mat-select>
            </mat-form-field>
          </div>
        </div>
        
        <!-- Warning Messages -->
        <div class="warnings" *ngIf="getWarnings().length > 0">
          <div class="warning-item" *ngFor="let warning of getWarnings()">
            <mat-icon color="warn">warning</mat-icon>
            <span>{{ warning }}</span>
          </div>
        </div>
        
        <!-- Preview Changes -->
        <div class="changes-preview" *ngIf="hasSelectedUpdates()">
          <h4>{{ 'TRANSACTIONS.PREVIEW_CHANGES' | translate }}</h4>
          <div class="preview-list">
            <div class="preview-item" *ngIf="bulkUpdateForm.get('updateCategory')?.value">
              <mat-icon>category</mat-icon>
              <span>
                {{ 'TRANSACTIONS.WILL_UPDATE_CATEGORY' | translate: { 
                  count: selectedTransactions.length, 
                  category: getSelectedCategoryName() 
                } }}
              </span>
            </div>
            
            <div class="preview-item" *ngIf="bulkUpdateForm.get('updateReconciled')?.value">
              <mat-icon>{{ bulkUpdateForm.get('isReconciled')?.value ? 'check_circle' : 'radio_button_unchecked' }}</mat-icon>
              <span>
                {{ bulkUpdateForm.get('isReconciled')?.value ? 
                   ('TRANSACTIONS.WILL_MARK_RECONCILED' | translate: { count: selectedTransactions.length }) :
                   ('TRANSACTIONS.WILL_MARK_UNRECONCILED' | translate: { count: selectedTransactions.length }) }}
              </span>
            </div>
            
            <div class="preview-item" *ngIf="bulkUpdateForm.get('updateAccount')?.value">
              <mat-icon>account_balance</mat-icon>
              <span>
                {{ 'TRANSACTIONS.WILL_UPDATE_ACCOUNT' | translate: { 
                  count: selectedTransactions.length, 
                  account: getSelectedAccountName() 
                } }}
              </span>
            </div>
            
            <div class="preview-item" *ngIf="bulkUpdateForm.get('updateTransactionType')?.value">
              <mat-icon>swap_horiz</mat-icon>
              <span>
                {{ 'TRANSACTIONS.WILL_CHANGE_TYPE' | translate: { 
                  count: selectedTransactions.length, 
                  type: getSelectedTransactionTypeName() 
                } }}
              </span>
            </div>
          </div>
        </div>
      </mat-dialog-content>
      
      <mat-dialog-actions align="end">
        <button mat-button type="button" mat-dialog-close [disabled]="isSubmitting">
          {{ 'COMMON.CANCEL' | translate }}
        </button>
        
        <button mat-raised-button 
                color="primary" 
                type="submit" 
                [disabled]="!hasSelectedUpdates() || isSubmitting">
          <mat-spinner diameter="20" *ngIf="isSubmitting"></mat-spinner>
          <mat-icon *ngIf="!isSubmitting">update</mat-icon>
          {{ 'TRANSACTIONS.APPLY_CHANGES' | translate }}
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .bulk-update-content {
      min-width: 500px;
      max-width: 600px;
    }
    
    .selected-summary {
      margin-bottom: 24px;
      padding: 16px;
      background: #f5f5f5;
      border-radius: 8px;
    }
    
    .selected-summary h4 {
      margin: 0 0 16px 0;
      color: #333;
    }
    
    .summary-stats {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
      flex-wrap: wrap;
    }
    
    .stat-item {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 14px;
      color: #666;
    }
    
    .income-icon {
      color: #4caf50;
    }
    
    .expense-icon {
      color: #f44336;
    }
    
    .transaction-preview {
      margin-top: 12px;
    }
    
    .income-chip {
      background: #e8f5e8;
      color: #2e7d2e;
    }
    
    .expense-chip {
      background: #ffebee;
      color: #c62828;
    }
    
    .transaction-desc {
      max-width: 200px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      margin-right: 8px;
    }
    
    .transaction-amount {
      font-weight: 600;
    }
    
    .more-indicator {
      margin-top: 8px;
      text-align: center;
      color: #666;
    }
    
    .update-options {
      margin-bottom: 24px;
    }
    
    .update-options h4 {
      margin: 0 0 8px 0;
      color: #333;
    }
    
    .update-hint {
      margin: 0 0 16px 0;
      color: #666;
      font-size: 14px;
    }
    
    .update-field {
      display: flex;
      flex-direction: column;
      gap: 8px;
      margin-bottom: 16px;
      padding: 12px;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
    }
    
    .update-field mat-checkbox {
      margin-bottom: 8px;
    }
    
    .field-input {
      margin-left: 24px;
      flex: 1;
    }
    
    .category-option,
    .account-option {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .account-info {
      display: flex;
      flex-direction: column;
    }
    
    .account-name {
      font-weight: 500;
    }
    
    .account-iban {
      font-size: 12px;
      color: #666;
    }
    
    .warnings {
      margin-bottom: 24px;
      padding: 12px;
      background: #fff3e0;
      border: 1px solid #ff9800;
      border-radius: 8px;
    }
    
    .warning-item {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 8px;
      color: #e65100;
      font-size: 14px;
    }
    
    .warning-item:last-child {
      margin-bottom: 0;
    }
    
    .changes-preview {
      margin-bottom: 16px;
      padding: 16px;
      background: #e3f2fd;
      border-radius: 8px;
    }
    
    .changes-preview h4 {
      margin: 0 0 12px 0;
      color: #1976d2;
    }
    
    .preview-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    
    .preview-item {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 14px;
      color: #1565c0;
    }
    
    mat-dialog-actions {
      padding: 16px 0;
      border-top: 1px solid #e0e0e0;
    }
    
    mat-spinner {
      display: inline-block;
      margin-right: 8px;
    }
    
    @media (max-width: 768px) {
      .bulk-update-content {
        min-width: auto;
        width: 95vw;
      }
      
      .summary-stats {
        flex-direction: column;
        gap: 8px;
      }
      
      .update-field {
        padding: 8px;
      }
      
      .field-input {
        margin-left: 0;
      }
    }
  `]
})
export class BulkUpdateDialogComponent implements OnInit, OnDestroy {
  bulkUpdateForm!: FormGroup;
  isSubmitting = false;
  
  selectedTransactions: Transaction[];
  categories: Category[];
  accounts: any[] = []; // Mock data
  
  previewTransactions: Transaction[] = [];
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<BulkUpdateDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: BulkUpdateDialogData,
    private transactionService: TransactionService,
    private translate: TranslateService
  ) {
    this.selectedTransactions = data.transactions;
    this.categories = data.categories;
    this.createForm();
  }

  ngOnInit(): void {
    this.previewTransactions = this.selectedTransactions.slice(0, 3);
    this.loadAccounts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): void {
    this.bulkUpdateForm = this.fb.group({
      // Field toggles
      updateCategory: [false],
      updateReconciled: [false],
      updateAccount: [false],
      updateTransactionType: [false],
      
      // Update values
      categoryId: [''],
      isReconciled: [false],
      accountId: [''],
      transactionType: ['']
    });
  }

  private loadAccounts(): void {
    // Mock data - in real app, load from account service
    this.accounts = [
      {
        id: '1',
        accountName: 'Girokonto',
        iban: 'DE89 3704 0044 0532 0130 00'
      },
      {
        id: '2',
        accountName: 'Sparkonto',
        iban: 'DE12 3704 0044 0532 0130 01'
      }
    ];
  }

  getIncomeCount(): number {
    return this.selectedTransactions.filter(t => t.transactionType === 'Income').length;
  }

  getExpenseCount(): number {
    return this.selectedTransactions.filter(t => t.transactionType === 'Expense').length;
  }

  getCategoryGroups(): { label: string; categories: Category[] }[] {
    const incomeCategories = this.categories.filter(c => c.categoryType === 'Income');
    const expenseCategories = this.categories.filter(c => c.categoryType === 'Expense');
    
    const groups = [];
    
    if (incomeCategories.length > 0) {
      groups.push({
        label: this.translate.instant('CATEGORY_TYPE.INCOME'),
        categories: incomeCategories
      });
    }
    
    if (expenseCategories.length > 0) {
      groups.push({
        label: this.translate.instant('CATEGORY_TYPE.EXPENSE'),
        categories: expenseCategories
      });
    }
    
    return groups;
  }

  canUpdateTransactionType(): boolean {
    // Only allow transaction type change if all selected transactions are of the same type
    const types = new Set(this.selectedTransactions.map(t => t.transactionType));
    return types.size === 1;
  }

  onFieldToggle(field: string): void {
    // Clear the related form control when toggling off
    const toggleControl = this.bulkUpdateForm.get(`update${field.charAt(0).toUpperCase() + field.slice(1)}`);
    const valueControl = this.bulkUpdateForm.get(field === 'reconciled' ? 'isReconciled' : 
                                                field === 'transactionType' ? 'transactionType' : 
                                                field + 'Id');
    
    if (toggleControl && valueControl && !toggleControl.value) {
      valueControl.setValue('');
    }
  }

  hasSelectedUpdates(): boolean {
    return this.bulkUpdateForm.get('updateCategory')?.value ||
           this.bulkUpdateForm.get('updateReconciled')?.value ||
           this.bulkUpdateForm.get('updateAccount')?.value ||
           this.bulkUpdateForm.get('updateTransactionType')?.value;
  }

  getWarnings(): string[] {
    const warnings: string[] = [];
    
    // Check for mixed transaction types when updating category
    if (this.bulkUpdateForm.get('updateCategory')?.value) {
      const types = new Set(this.selectedTransactions.map(t => t.transactionType));
      if (types.size > 1) {
        warnings.push(this.translate.instant('TRANSACTIONS.WARNING_MIXED_TYPES_CATEGORY'));
      }
    }
    
    // Check for transaction type change impact
    if (this.bulkUpdateForm.get('updateTransactionType')?.value) {
      warnings.push(this.translate.instant('TRANSACTIONS.WARNING_TYPE_CHANGE_IMPACT'));
    }
    
    return warnings;
  }

  getSelectedCategoryName(): string {
    const categoryId = this.bulkUpdateForm.get('categoryId')?.value;
    const category = this.categories.find(c => c.id === categoryId);
    return category ? category.name : '';
  }

  getSelectedAccountName(): string {
    const accountId = this.bulkUpdateForm.get('accountId')?.value;
    const account = this.accounts.find(a => a.id === accountId);
    return account ? account.accountName : '';
  }

  getSelectedTransactionTypeName(): string {
    const type = this.bulkUpdateForm.get('transactionType')?.value;
    return type ? this.translate.instant(`TRANSACTIONS.${type.toUpperCase()}`) : '';
  }

  onSubmit(): void {
    if (!this.hasSelectedUpdates()) {
      return;
    }
    
    this.isSubmitting = true;
    
    const updateData: any = {};
    
    // Build update object based on selected fields
    if (this.bulkUpdateForm.get('updateCategory')?.value) {
      updateData.categoryId = this.bulkUpdateForm.get('categoryId')?.value;
    }
    
    if (this.bulkUpdateForm.get('updateReconciled')?.value) {
      updateData.isReconciled = this.bulkUpdateForm.get('isReconciled')?.value;
    }
    
    if (this.bulkUpdateForm.get('updateAccount')?.value) {
      updateData.accountId = this.bulkUpdateForm.get('accountId')?.value;
    }
    
    if (this.bulkUpdateForm.get('updateTransactionType')?.value) {
      updateData.transactionType = this.bulkUpdateForm.get('transactionType')?.value;
    }
    
    const bulkUpdateRequest: BulkUpdateTransactionsDto = {
      transactionIds: this.selectedTransactions.map(t => t.id),
      updates: updateData
    };
    
    this.transactionService.bulkUpdateTransactions(bulkUpdateRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedCount) => {
          this.isSubmitting = false;
          this.dialogRef.close({ success: true, updatedCount });
        },
        error: (error) => {
          console.error('Error bulk updating transactions:', error);
          this.isSubmitting = false;
          // Handle error - could show error in dialog instead of closing
        }
      });
  }
}
