import { Component, OnInit, Inject, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, Observable } from 'rxjs';
import { takeUntil, startWith, map, debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { TransactionService } from '../../../../core/services/transaction.service';
import { CategoryService } from '../../../../core/services/category.service';
import { FileService } from '../../../../core/services/file.service';
import {
  Transaction,
  CreateTransactionDto,
  UpdateTransactionDto,
  Category,
  CategoryGroup,
  TransactionType,
  CategoryType,
  VATCalculation,
  CategorySuggestion,
  Account
} from '../../../../core/models/transaction.model';
import { GermanFormatService } from '../../../../shared/services/german-format.service';
import { GermanCurrencyPipe } from '../../../../shared/pipes/german-currency.pipe';
import { GermanNumberPipe } from '../../../../shared/pipes/german-number.pipe';
import { GermanValidators } from '../../../../shared/validators/german-validators';

export interface TransactionFormData {
  transaction: Transaction | null;
  isEdit: boolean;
}

@Component({
  selector: 'app-transaction-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatCheckboxModule,
    MatChipsModule,
    MatAutocompleteModule,
    TranslateModule,
    GermanCurrencyPipe,
    GermanNumberPipe
  ],
  template: `
    <form [formGroup]="transactionForm" (ngSubmit)="onSubmit()" class="transaction-form">
      <h2 mat-dialog-title>
        {{ isEditMode ? ('TRANSACTIONS.EDIT_TRANSACTION' | translate) : ('TRANSACTIONS.ADD_TRANSACTION' | translate) }}
      </h2>
      
      <mat-dialog-content class="form-content">
        <!-- Transaction Type Selection -->
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ 'TRANSACTIONS.TYPE' | translate }}</mat-label>
            <mat-select formControlName="transactionType" (selectionChange)="onTransactionTypeChange()">
              <mat-option value="Income">
                <div class="transaction-type-option">
                  <mat-icon class="income-icon">trending_up</mat-icon>
                  <span>{{ 'TRANSACTIONS.INCOME' | translate }}</span>
                </div>
              </mat-option>
              <mat-option value="Expense">
                <div class="transaction-type-option">
                  <mat-icon class="expense-icon">trending_down</mat-icon>
                  <span>{{ 'TRANSACTIONS.EXPENSE' | translate }}</span>
                </div>
              </mat-option>
            </mat-select>
            <mat-error *ngIf="transactionForm.get('transactionType')?.hasError('required')">
              {{ 'VALIDATION.REQUIRED' | translate }}
            </mat-error>
          </mat-form-field>
        </div>
        
        <!-- Amount and Date -->
        <div class="form-row">
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>{{ 'TRANSACTIONS.AMOUNT' | translate }}</mat-label>
            <input matInput 
                   type="text" 
                   formControlName="amount" 
                   class="currency-input"
                   [placeholder]="'1.234,56'"
                   (input)="onAmountChange()"
                   (blur)="formatAmountInput()">
            <span matSuffix>€</span>
            <mat-hint>{{ 'TRANSACTIONS.AMOUNT_HINT' | translate }}</mat-hint>
            <mat-error *ngIf="transactionForm.get('amount')?.hasError('required')">
              {{ 'VALIDATION.REQUIRED' | translate }}
            </mat-error>
            <mat-error *ngIf="transactionForm.get('amount')?.hasError('min')">
              {{ 'VALIDATION.MIN_AMOUNT' | translate }}
            </mat-error>
            <mat-error *ngIf="transactionForm.get('amount')?.hasError('pattern')">
              {{ 'VALIDATION.INVALID_AMOUNT_FORMAT' | translate }}
            </mat-error>
          </mat-form-field>
          
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>{{ 'TRANSACTIONS.DATE' | translate }}</mat-label>
            <input matInput 
                   [matDatepicker]="datePicker" 
                   formControlName="transactionDate"
                   readonly>
            <mat-datepicker-toggle matSuffix [for]="datePicker"></mat-datepicker-toggle>
            <mat-datepicker #datePicker></mat-datepicker>
            <mat-error *ngIf="transactionForm.get('transactionDate')?.hasError('required')">
              {{ 'VALIDATION.REQUIRED' | translate }}
            </mat-error>
          </mat-form-field>
        </div>
        
        <!-- Account Selection -->
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ 'TRANSACTIONS.ACCOUNT' | translate }}</mat-label>
            <mat-select formControlName="accountId">
              <mat-option *ngFor="let account of accounts" [value]="account.id">
                <div class="account-option">
                  <mat-icon>account_balance</mat-icon>
                  <div class="account-info">
                    <span class="account-name">{{ account.accountName }}</span>
                    <small class="account-details">{{ account.iban }} • {{ account.bankName }}</small>
                  </div>
                </div>
              </mat-option>
            </mat-select>
            <mat-error *ngIf="transactionForm.get('accountId')?.hasError('required')">
              {{ 'VALIDATION.ACCOUNT_REQUIRED' | translate }}
            </mat-error>
          </mat-form-field>
        </div>
        
        <!-- Category Selection with Suggestions -->
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ 'TRANSACTIONS.CATEGORY' | translate }}</mat-label>
            <mat-select formControlName="categoryId" (selectionChange)="onCategoryChange()">
              <mat-optgroup *ngFor="let group of filteredCategoryGroups" [label]="group.label">
                <mat-option *ngFor="let category of group.categories" [value]="category.id">
                  <div class="category-option">
                    <mat-icon [style.color]="category.color">{{ category.icon }}</mat-icon>
                    <div class="category-info">
                      <span>{{ category.name }}</span>
                      <small class="vat-rate">MwSt: {{ (category.defaultVatRate * 100) | germanNumber:'1.0-1' }}%</small>
                    </div>
                  </div>
                </mat-option>
              </mat-optgroup>
            </mat-select>
            <mat-error *ngIf="transactionForm.get('categoryId')?.hasError('required')">
              {{ 'VALIDATION.CATEGORY_REQUIRED' | translate }}
            </mat-error>
          </mat-form-field>
        </div>
        
        <!-- Category Suggestions -->
        <div class="category-suggestions" *ngIf="categorySuggestions.length > 0">
          <h4>{{ 'TRANSACTIONS.SUGGESTED_CATEGORIES' | translate }}</h4>
          <div class="suggestion-chips">
            <mat-chip-listbox>
              <mat-chip *ngFor="let suggestion of categorySuggestions" 
                       (click)="applySuggestion(suggestion)"
                       [class.selected]="transactionForm.get('categoryId')?.value === suggestion.categoryId">
                <mat-icon [style.color]="suggestion.categoryColor">{{ suggestion.categoryIcon }}</mat-icon>
                <span>{{ suggestion.categoryName }}</span>
                <small>({{ (suggestion.confidenceScore * 100) | germanNumber:'1.0-0' }}%)</small>
              </mat-chip>
            </mat-chip-listbox>
          </div>
        </div>
        
        <!-- Description -->
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ 'TRANSACTIONS.DESCRIPTION' | translate }}</mat-label>
            <input matInput 
                   formControlName="description" 
                   maxlength="500"
                   (input)="onDescriptionChange()"
                   [placeholder]="'TRANSACTIONS.DESCRIPTION_PLACEHOLDER' | translate">
            <mat-hint align="end">{{ getDescriptionLength() }}/500</mat-hint>
            <mat-error *ngIf="transactionForm.get('description')?.hasError('required')">
              {{ 'VALIDATION.REQUIRED' | translate }}
            </mat-error>
          </mat-form-field>
        </div>
        
        <!-- Merchant Name -->
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ 'TRANSACTIONS.MERCHANT_NAME' | translate }}</mat-label>
            <input matInput 
                   formControlName="merchantName"
                   (input)="onMerchantChange()"
                   [placeholder]="'TRANSACTIONS.MERCHANT_PLACEHOLDER' | translate">
            <mat-icon matSuffix>store</mat-icon>
          </mat-form-field>
        </div>
        
        <!-- Reference Number -->
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ 'TRANSACTIONS.REFERENCE_NUMBER' | translate }}</mat-label>
            <input matInput formControlName="referenceNumber">
            <mat-icon matSuffix>receipt</mat-icon>
          </mat-form-field>
        </div>
        
        <!-- VAT Calculation Display -->
        <mat-card class="vat-card" *ngIf="vatCalculation && vatCalculation.vatAmount > 0">
          <mat-card-header>
            <mat-card-title>{{ 'TRANSACTIONS.VAT_BREAKDOWN' | translate }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="vat-breakdown">
              <div class="vat-row">
                <span>{{ 'TRANSACTIONS.GROSS_AMOUNT' | translate }}:</span>
                <span class="amount">{{ vatCalculation.grossAmount | germanCurrency }}</span>
              </div>
              <div class="vat-row">
                <span>{{ 'TRANSACTIONS.NET_AMOUNT' | translate }}:</span>
                <span class="amount">{{ vatCalculation.netAmount | germanCurrency }}</span>
              </div>
              <div class="vat-row highlight">
                <span>{{ 'TRANSACTIONS.VAT_AMOUNT' | translate }} ({{ (vatCalculation.vatRate * 100) | germanNumber:'1.0-1' }}%):</span>
                <span class="amount vat-amount">{{ vatCalculation.vatAmount | germanCurrency }}</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
        
        <!-- Recurring Transaction Options -->
        <div class="form-row">
          <mat-checkbox formControlName="isRecurring" (change)="onRecurringChange()">
            {{ 'TRANSACTIONS.IS_RECURRING' | translate }}
          </mat-checkbox>
        </div>
        
        <div class="recurring-options" *ngIf="transactionForm.get('isRecurring')?.value">
          <div class="form-row">
            <mat-form-field appearance="outline" class="half-width">
              <mat-label>{{ 'TRANSACTIONS.RECURRING_FREQUENCY' | translate }}</mat-label>
              <mat-select formControlName="recurringFrequency">
                <mat-option value="Weekly">{{ 'FREQUENCY.WEEKLY' | translate }}</mat-option>
                <mat-option value="Monthly">{{ 'FREQUENCY.MONTHLY' | translate }}</mat-option>
                <mat-option value="Quarterly">{{ 'FREQUENCY.QUARTERLY' | translate }}</mat-option>
                <mat-option value="Yearly">{{ 'FREQUENCY.YEARLY' | translate }}</mat-option>
              </mat-select>
            </mat-form-field>
            
            <mat-form-field appearance="outline" class="half-width">
              <mat-label>{{ 'TRANSACTIONS.RECURRING_END_DATE' | translate }}</mat-label>
              <input matInput [matDatepicker]="endDatePicker" formControlName="recurringEndDate">
              <mat-datepicker-toggle matSuffix [for]="endDatePicker"></mat-datepicker-toggle>
              <mat-datepicker #endDatePicker></mat-datepicker>
            </mat-form-field>
          </div>
        </div>
        
        <!-- Notes -->
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ 'TRANSACTIONS.NOTES' | translate }}</mat-label>
            <textarea matInput 
                     formControlName="notes" 
                     rows="3"
                     maxlength="1000"
                     [placeholder]="'TRANSACTIONS.NOTES_PLACEHOLDER' | translate"></textarea>
            <mat-hint align="end">{{ getNotesLength() }}/1000</mat-hint>
          </mat-form-field>
        </div>
      </mat-dialog-content>
      
      <mat-dialog-actions align="end" class="dialog-actions">
        <button mat-button type="button" mat-dialog-close [disabled]="isSubmitting">
          {{ 'COMMON.CANCEL' | translate }}
        </button>
        
        <button mat-button 
                type="button" 
                *ngIf="!isEditMode"
                (click)="saveAsDraft()"
                [disabled]="isSubmitting">
          {{ 'TRANSACTIONS.SAVE_DRAFT' | translate }}
        </button>
        
        <button mat-raised-button 
                color="primary" 
                type="submit" 
                [disabled]="transactionForm.invalid || isSubmitting">
          <mat-spinner diameter="20" *ngIf="isSubmitting"></mat-spinner>
          <mat-icon *ngIf="!isSubmitting">{{ isEditMode ? 'save' : 'add' }}</mat-icon>
          {{ isEditMode ? ('COMMON.UPDATE' | translate) : ('COMMON.CREATE' | translate) }}
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .transaction-form {
      max-width: 800px;
    }
    
    .form-content {
      padding: 0 !important;
      margin: 0 !important;
    }
    
    .form-row {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
      align-items: flex-start;
    }
    
    .full-width {
      width: 100%;
    }
    
    .half-width {
      width: calc(50% - 8px);
    }
    
    .transaction-type-option,
    .category-option,
    .account-option {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .category-info,
    .account-info {
      display: flex;
      flex-direction: column;
    }
    
    .vat-rate,
    .account-details {
      font-size: 12px;
      color: #666;
    }
    
    .account-name {
      font-weight: 500;
    }
    
    .income-icon {
      color: #4caf50;
    }
    
    .expense-icon {
      color: #f44336;
    }
    
    .currency-input {
      text-align: right;
    }
    
    .category-suggestions {
      margin: 16px 0;
      padding: 16px;
      background: #f5f5f5;
      border-radius: 8px;
    }
    
    .category-suggestions h4 {
      margin: 0 0 12px 0;
      font-size: 14px;
      color: #333;
    }
    
    .suggestion-chips mat-chip {
      margin-right: 8px;
      margin-bottom: 8px;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    
    .suggestion-chips mat-chip:hover {
      background-color: #e3f2fd;
      transform: translateY(-1px);
    }
    
    .suggestion-chips mat-chip.selected {
      background-color: #2196f3;
      color: white;
    }
    
    .suggestion-chips mat-chip mat-icon {
      margin-right: 4px;
    }
    
    .suggestion-chips mat-chip small {
      margin-left: 4px;
      opacity: 0.8;
    }
    
    .vat-card {
      margin: 16px 0;
    }
    
    .vat-breakdown {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    
    .vat-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 4px 0;
    }
    
    .vat-row.highlight {
      border-top: 1px solid #e0e0e0;
      padding-top: 8px;
      font-weight: 500;
    }
    
    .vat-amount {
      color: #1976d2;
      font-weight: 600;
    }
    
    .recurring-options {
      margin-left: 24px;
      padding: 16px;
      background: #f9f9f9;
      border-radius: 8px;
      border-left: 4px solid #2196f3;
    }
    
    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e0e0e0;
      gap: 12px;
    }
    
    .dialog-actions button {
      min-width: 80px;
    }
    
    mat-spinner {
      display: inline-block;
      margin-right: 8px;
    }
    
    @media (max-width: 768px) {
      .form-row {
        flex-direction: column;
      }
      
      .half-width {
        width: 100%;
      }
      
      .dialog-actions {
        flex-direction: column;
      }
      
      .dialog-actions button {
        width: 100%;
        margin: 0;
      }
    }
  `]
})
export class TransactionFormComponent implements OnInit, OnDestroy {
  // Input properties
  @Input() mode: 'create' | 'edit' = 'create';
  @Input() transaction?: Transaction;
  
  // Output events  
  @Output() transactionSaved = new EventEmitter<Transaction>();
  
  transactionForm!: FormGroup;
  isEditMode = false;
  isSubmitting = false;
  
  // Data
  categories: Category[] = [];
  filteredCategoryGroups: CategoryGroup[] = [];
  categorySuggestions: CategorySuggestion[] = [];
  accounts: Account[] = [];
  vatCalculation?: VATCalculation;
  
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<TransactionFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TransactionFormData,
    private transactionService: TransactionService,
    private categoryService: CategoryService,
    private fileService: FileService,
    private germanFormat: GermanFormatService,
    private translate: TranslateService
  ) {
    this.isEditMode = data.isEdit;
    this.createForm();
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadAccounts();
    
    if (this.isEditMode && this.data.transaction) {
      this.populateForm(this.data.transaction);
    } else {
      // Set default values
      this.transactionForm.patchValue({
        transactionDate: new Date(),
        transactionType: 'Expense'
      });
    }
    
    this.onTransactionTypeChange();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): void {
    this.transactionForm = this.fb.group({
      accountId: ['', Validators.required],
      categoryId: ['', Validators.required],
      amount: ['', [Validators.required, GermanValidators.germanCurrency, Validators.min(0.01)]],
      description: ['', [Validators.required, Validators.maxLength(500)]],
      merchantName: [''],
      transactionDate: ['', Validators.required],
      transactionType: ['', Validators.required],
      notes: ['', Validators.maxLength(1000)],
      referenceNumber: [''],
      isRecurring: [false],
      recurringFrequency: [''],
      recurringEndDate: ['']
    });
  }

  private loadCategories(): void {
    this.categoryService.getCategoryHierarchy().subscribe({
      next: (hierarchies) => {
        this.filteredCategoryGroups = hierarchies.map(h => ({
          label: this.translate.instant(`CATEGORY_TYPE.${h.categoryType.toUpperCase()}`),
          categoryType: h.categoryType as CategoryType,
          categories: this.flattenCategories(h.categories)
        }));
        
        this.categories = this.filteredCategoryGroups.flatMap(g => g.categories);
        this.filterCategoriesByType();
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  private loadAccounts(): void {
    // This would load from an account service
    // For now, using mock data
    this.accounts = [
      {
        id: '1',
        userId: 'user1',
        accountName: 'Girokonto',
        accountType: 'Checking',
        iban: 'DE89 3704 0044 0532 0130 00',
        bankName: 'Deutsche Bank',
        currency: 'EUR',
        balance: 2500.00,
        isActive: true,
        createdAt: new Date(),
        updatedAt: new Date()
      }
    ];
  }

  private flattenCategories(categories: Category[]): Category[] {
    let flattened: Category[] = [];
    
    categories.forEach(category => {
      flattened.push(category);
      if (category.subCategories && category.subCategories.length > 0) {
        flattened = flattened.concat(this.flattenCategories(category.subCategories));
      }
    });
    
    return flattened;
  }

  private populateForm(transaction: Transaction): void {
    this.transactionForm.patchValue({
      accountId: transaction.accountId,
      categoryId: transaction.categoryId,
      amount: this.germanFormat.formatNumber(transaction.amount, 2),
      description: transaction.description,
      merchantName: transaction.merchantName,
      transactionDate: new Date(transaction.transactionDate),
      transactionType: transaction.transactionType,
      notes: transaction.notes,
      referenceNumber: transaction.referenceNumber,
      isRecurring: transaction.isRecurring,
      recurringFrequency: transaction.recurringFrequency,
      recurringEndDate: transaction.recurringEndDate ? new Date(transaction.recurringEndDate) : null
    });
    
    this.calculateVAT();
  }

  onTransactionTypeChange(): void {
    const transactionType = this.transactionForm.get('transactionType')?.value;
    this.filterCategoriesByType();
    
    // Clear category selection if it doesn't match the new type
    const selectedCategoryId = this.transactionForm.get('categoryId')?.value;
    if (selectedCategoryId) {
      const selectedCategory = this.categories.find(c => c.id === selectedCategoryId);
      if (selectedCategory) {
        const expectedCategoryType = transactionType === 'Income' ? CategoryType.Income : CategoryType.Expense;
        if (selectedCategory.categoryType !== expectedCategoryType) {
          this.transactionForm.get('categoryId')?.setValue('');
        }
      }
    }
  }

  private filterCategoriesByType(): void {
    const transactionType = this.transactionForm.get('transactionType')?.value;
    const expectedCategoryType = transactionType === 'Income' ? CategoryType.Income : CategoryType.Expense;
    
    this.filteredCategoryGroups = this.filteredCategoryGroups.map(group => ({
      ...group,
      categories: group.categories.filter(c => c.categoryType === expectedCategoryType)
    })).filter(group => group.categories.length > 0);
  }

  onAmountChange(): void {
    this.calculateVAT();
  }

  onCategoryChange(): void {
    this.calculateVAT();
  }

  onDescriptionChange(): void {
    this.getSuggestions();
  }

  onMerchantChange(): void {
    this.getSuggestions();
  }

  onRecurringChange(): void {
    const isRecurring = this.transactionForm.get('isRecurring')?.value;
    
    if (isRecurring) {
      this.transactionForm.get('recurringFrequency')?.setValidators([Validators.required]);
    } else {
      this.transactionForm.get('recurringFrequency')?.clearValidators();
      this.transactionForm.get('recurringFrequency')?.setValue('');
      this.transactionForm.get('recurringEndDate')?.setValue('');
    }
    
    this.transactionForm.get('recurringFrequency')?.updateValueAndValidity();
  }

  private getSuggestions(): void {
    const description = this.transactionForm.get('description')?.value;
    const merchantName = this.transactionForm.get('merchantName')?.value;
    const amountStr = this.transactionForm.get('amount')?.value;
    const transactionType = this.transactionForm.get('transactionType')?.value;
    
    if (!description || description.length < 3) {
      this.categorySuggestions = [];
      return;
    }
    
    const amount = this.germanFormat.parseGermanNumber(amountStr);
    
    this.categoryService.suggestCategory({
      description,
      merchantName: merchantName || undefined,
      amount: amount > 0 ? amount : undefined,
      transactionType: transactionType as TransactionType
    }).subscribe({
      next: (suggestions) => {
        this.categorySuggestions = suggestions.slice(0, 5); // Limit to top 5
      },
      error: (error) => {
        console.error('Error getting category suggestions:', error);
        this.categorySuggestions = [];
      }
    });
  }

  applySuggestion(suggestion: CategorySuggestion): void {
    this.transactionForm.get('categoryId')?.setValue(suggestion.categoryId);
    this.calculateVAT();
  }

  private calculateVAT(): void {
    const amountStr = this.transactionForm.get('amount')?.value;
    const categoryId = this.transactionForm.get('categoryId')?.value;
    
    if (!amountStr || !categoryId) {
      this.vatCalculation = undefined;
      return;
    }
    
    const amount = this.germanFormat.parseGermanNumber(amountStr);
    const category = this.categories.find(c => c.id === categoryId);
    
    if (amount > 0 && category && category.defaultVatRate > 0) {
      this.vatCalculation = this.transactionService.calculateVAT(amount, category.defaultVatRate);
    } else {
      this.vatCalculation = undefined;
    }
  }

  formatAmountInput(): void {
    const amountStr = this.transactionForm.get('amount')?.value;
    if (amountStr) {
      const amount = this.germanFormat.parseGermanNumber(amountStr);
      if (amount > 0) {
        const formatted = this.germanFormat.formatNumber(amount, 2);
        this.transactionForm.get('amount')?.setValue(formatted, { emitEvent: false });
      }
    }
  }

  getDescriptionLength(): number {
    return this.transactionForm.get('description')?.value?.length || 0;
  }

  getNotesLength(): number {
    return this.transactionForm.get('notes')?.value?.length || 0;
  }

  saveAsDraft(): void {
    // Implement draft saving functionality
    console.log('Saving as draft...');
  }

  onSubmit(): void {
    if (this.transactionForm.valid) {
      this.isSubmitting = true;
      
      const formValue = this.transactionForm.value;
      
      // Convert German formatted amount to number
      const amount = this.germanFormat.parseGermanNumber(formValue.amount);
      
      const transactionData = {
        ...formValue,
        amount,
        transactionDate: formValue.transactionDate.toISOString()
      };
      
      // Remove empty optional fields
      Object.keys(transactionData).forEach(key => {
        if (transactionData[key] === '' || transactionData[key] === null) {
          delete transactionData[key];
        }
      });
      
      const request$ = this.isEditMode 
        ? this.transactionService.updateTransaction(this.data.transaction!.id, transactionData as UpdateTransactionDto)
        : this.transactionService.createTransaction(transactionData as CreateTransactionDto);
      
      request$.subscribe({
        next: (transaction) => {
          this.isSubmitting = false;
          this.dialogRef.close(transaction);
        },
        error: (error) => {
          console.error('Error saving transaction:', error);
          this.isSubmitting = false;
          // Handle error - show snackbar, etc.
        }
      });
    }
  }
}
