import { Component, OnInit, ViewChild, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SelectionModel } from '@angular/cdk/collections';
import { Subject, Observable } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { TransactionService } from '../../../../core/services/transaction.service';
import { CategoryService } from '../../../../core/services/category.service';
import { FileService } from '../../../../core/services/file.service';
import { LoadingService } from '../../../../core/services/loading.service';
import {
  Transaction,
  Category,
  TransactionQueryParameters,
  TransactionType,
  CategoryType
} from '../../../../core/models/transaction.model';
import { PagedResult } from '../../../../core/models/api-response.model';
import { GermanCurrencyPipe } from '../../../../shared/pipes/german-currency.pipe';
import { GermanDatePipe } from '../../../../shared/pipes/german-date.pipe';
import { GermanNumberPipe } from '../../../../shared/pipes/german-number.pipe';
import { TransactionFormComponent } from '../transaction-form/transaction-form.component';
import { PdfUploadComponent } from '../pdf-upload/pdf-upload.component';
import { BulkUpdateDialogComponent } from '../bulk-update-dialog/bulk-update-dialog.component';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { MatTooltip } from '@angular/material/tooltip';
import { MatDivider } from '@angular/material/divider';

@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatMenuModule,
    MatSnackBarModule,
    MatTooltip,
    MatDivider,
    MatDialogModule,
    MatChipsModule,
    TranslateModule,
    GermanCurrencyPipe,
    GermanDatePipe,
    GermanNumberPipe
  ],
  template: `
    <div class="transaction-container">
      <!-- Header with filters -->
      <mat-card class="filter-card">
        <mat-card-header>
          <mat-card-title>{{ 'TRANSACTIONS.TITLE' | translate }}</mat-card-title>
          <div class="header-actions">
            <button mat-raised-button color="primary" (click)="openCreateDialog()">
              <mat-icon>add</mat-icon>
              {{ 'TRANSACTIONS.ADD_NEW' | translate }}
            </button>
          </div>
        </mat-card-header>
        <mat-card-content>
          <div class="filter-row">
            <mat-form-field appearance="outline" class="search-field">
              <mat-label>{{ 'TRANSACTIONS.SEARCH' | translate }}</mat-label>
              <input matInput [(ngModel)]="searchQuery" (input)="onSearchChange()" 
                     [placeholder]="'TRANSACTIONS.SEARCH_PLACEHOLDER' | translate">
              <mat-icon matSuffix>search</mat-icon>
            </mat-form-field>
            
            <mat-form-field appearance="outline">
              <mat-label>{{ 'TRANSACTIONS.TYPE' | translate }}</mat-label>
              <mat-select [(value)]="selectedTransactionType" (selectionChange)="applyFilters()">
                <mat-option value="">{{ 'COMMON.ALL' | translate }}</mat-option>
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
            
            <mat-form-field appearance="outline">
              <mat-label>{{ 'TRANSACTIONS.CATEGORY' | translate }}</mat-label>
              <mat-select [(value)]="selectedCategoryId" (selectionChange)="applyFilters()">
                <mat-option value="">{{ 'COMMON.ALL' | translate }}</mat-option>
                <mat-optgroup *ngFor="let group of categoryGroups" [label]="group.label">
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
          
          <div class="filter-row">
            <mat-form-field appearance="outline">
              <mat-label>{{ 'TRANSACTIONS.DATE_FROM' | translate }}</mat-label>
              <input matInput [matDatepicker]="fromPicker" [(ngModel)]="dateFrom" 
                     (dateChange)="applyFilters()">
              <mat-datepicker-toggle matSuffix [for]="fromPicker"></mat-datepicker-toggle>
              <mat-datepicker #fromPicker></mat-datepicker>
            </mat-form-field>
            
            <mat-form-field appearance="outline">
              <mat-label>{{ 'TRANSACTIONS.DATE_TO' | translate }}</mat-label>
              <input matInput [matDatepicker]="toPicker" [(ngModel)]="dateTo" 
                     (dateChange)="applyFilters()">
              <mat-datepicker-toggle matSuffix [for]="toPicker"></mat-datepicker-toggle>
              <mat-datepicker #toPicker></mat-datepicker>
            </mat-form-field>
            
            <mat-form-field appearance="outline">
              <mat-label>{{ 'TRANSACTIONS.MIN_AMOUNT' | translate }}</mat-label>
              <input matInput type="number" step="0.01" [(ngModel)]="minAmount" 
                     (input)="applyFilters()" [placeholder]="'0,00'">
              <span matSuffix>€</span>
            </mat-form-field>
            
            <mat-form-field appearance="outline">
              <mat-label>{{ 'TRANSACTIONS.MAX_AMOUNT' | translate }}</mat-label>
              <input matInput type="number" step="0.01" [(ngModel)]="maxAmount" 
                     (input)="applyFilters()" [placeholder]="'0,00'">
              <span matSuffix>€</span>
            </mat-form-field>
            
            <button mat-raised-button (click)="clearFilters()">
              <mat-icon>clear</mat-icon>
              {{ 'COMMON.CLEAR' | translate }}
            </button>
          </div>
        </mat-card-content>
      </mat-card>
      
      <!-- Transaction Table -->
      <mat-card class="transactions-table-card">
        <div class="table-actions">
          <div class="bulk-actions" *ngIf="selection.hasValue()">
            <span class="selection-info">
              {{ getSelectionText() }}
            </span>
            <button mat-raised-button (click)="openBulkUpdateDialog()" 
                    [disabled]="!selection.hasValue()">
              <mat-icon>edit</mat-icon>
              {{ 'TRANSACTIONS.BULK_UPDATE' | translate }}
            </button>
            <button mat-raised-button color="warn" (click)="bulkDeleteTransactions()" 
                    [disabled]="!selection.hasValue()">
              <mat-icon>delete</mat-icon>
              {{ 'TRANSACTIONS.BULK_DELETE' | translate }}
            </button>
          </div>
          
          <div class="table-info">
            <span class="total-info">
              {{ 'TRANSACTIONS.TOTAL_COUNT' | translate }}: {{ totalCount }}
            </span>
            <button mat-icon-button (click)="exportTransactions()" 
                    matTooltip="{{ 'TRANSACTIONS.EXPORT' | translate }}">
              <mat-icon>download</mat-icon>
            </button>
            <button mat-icon-button (click)="refreshData()" 
                    matTooltip="{{ 'COMMON.REFRESH' | translate }}">
              <mat-icon>refresh</mat-icon>
            </button>
          </div>
        </div>
        
        <mat-table [dataSource]="dataSource" class="transactions-table" matSort>
          <ng-container matColumnDef="select">
            <mat-header-cell *matHeaderCellDef>
              <mat-checkbox (change)="masterToggle()" 
                           [checked]="selection.hasValue() && isAllSelected()" 
                           [indeterminate]="selection.hasValue() && !isAllSelected()">
              </mat-checkbox>
            </mat-header-cell>
            <mat-cell *matCellDef="let transaction">
              <mat-checkbox (click)="$event.stopPropagation()" 
                           [checked]="selection.isSelected(transaction)" 
                           (change)="toggleSelection(transaction)">
              </mat-checkbox>
            </mat-cell>
          </ng-container>
          
          <ng-container matColumnDef="date">
            <mat-header-cell *matHeaderCellDef mat-sort-header>
              {{ 'TRANSACTIONS.DATE' | translate }}
            </mat-header-cell>
            <mat-cell *matCellDef="let transaction">
              <div class="date-cell">
                <span class="date">{{ transaction.transactionDate | germanDate }}</span>
                <small class="time">{{ transaction.createdAt | date:'HH:mm':'de' }}</small>
              </div>
            </mat-cell>
          </ng-container>
          
          <ng-container matColumnDef="description">
            <mat-header-cell *matHeaderCellDef>
              {{ 'TRANSACTIONS.DESCRIPTION' | translate }}
            </mat-header-cell>
            <mat-cell *matCellDef="let transaction" class="description-cell">
              <div class="transaction-description">
                <span class="description-text" [matTooltip]="transaction.description">
                  {{ transaction.description }}
                </span>
                <div class="transaction-meta" *ngIf="transaction.merchantName || transaction.referenceNumber">
                  <mat-chip-listbox>
                    <mat-chip *ngIf="transaction.merchantName" class="merchant-chip">
                      <mat-icon matChipAvatar>store</mat-icon>
                      {{ transaction.merchantName }}
                    </mat-chip>
                    <mat-chip *ngIf="transaction.referenceNumber" class="reference-chip">
                      <mat-icon matChipAvatar>receipt</mat-icon>
                      {{ transaction.referenceNumber }}
                    </mat-chip>
                  </mat-chip-listbox>
                </div>
              </div>
            </mat-cell>
          </ng-container>
          
          <ng-container matColumnDef="category">
            <mat-header-cell *matHeaderCellDef>
              {{ 'TRANSACTIONS.CATEGORY' | translate }}
            </mat-header-cell>
            <mat-cell *matCellDef="let transaction">
              <div class="category-cell" *ngIf="transaction.category">
                <div class="category-chip">
                  <mat-icon [style.color]="transaction.category.color">
                    {{ transaction.category.icon }}
                  </mat-icon>
                  <span>{{ transaction.category.name }}</span>
                </div>
              </div>
              <div class="no-category" *ngIf="!transaction.category">
                <mat-icon color="warn">warning</mat-icon>
                <span>{{ 'TRANSACTIONS.NO_CATEGORY' | translate }}</span>
              </div>
            </mat-cell>
          </ng-container>
          
          <ng-container matColumnDef="amount">
            <mat-header-cell *matHeaderCellDef mat-sort-header class="amount-header">
              {{ 'TRANSACTIONS.AMOUNT' | translate }}
            </mat-header-cell>
            <mat-cell *matCellDef="let transaction" class="amount-cell">
              <div class="amount-display" 
                   [class.income]="transaction.transactionType === 'Income'"
                   [class.expense]="transaction.transactionType === 'Expense'">
                <span class="gross-amount">
                  {{ transaction.transactionType === 'Income' ? '+' : '-' }}{{ transaction.amount | germanCurrency }}
                </span>
                <div class="vat-info" *ngIf="transaction.vatAmount > 0">
                  <small class="net-amount">
                    {{ 'TRANSACTIONS.NET' | translate }}: 
                    {{ transaction.netAmount | germanCurrency }}
                  </small>
                  <small class="vat-amount">
                    {{ 'TRANSACTIONS.VAT' | translate }} ({{ transaction.vatRate | germanNumber:'1.0-1' }}%): 
                    {{ transaction.vatAmount | germanCurrency }}
                  </small>
                </div>
              </div>
            </mat-cell>
          </ng-container>
          
          <ng-container matColumnDef="account">
            <mat-header-cell *matHeaderCellDef>
              {{ 'TRANSACTIONS.ACCOUNT' | translate }}
            </mat-header-cell>
            <mat-cell *matCellDef="let transaction">
              <div class="account-cell" *ngIf="transaction.account">
                <mat-icon>account_balance</mat-icon>
                <span>{{ transaction.account.accountName }}</span>
              </div>
            </mat-cell>
          </ng-container>
          
          <ng-container matColumnDef="status">
            <mat-header-cell *matHeaderCellDef>
              {{ 'TRANSACTIONS.STATUS' | translate }}
            </mat-header-cell>
            <mat-cell *matCellDef="let transaction">
              <div class="status-indicators">
                <mat-icon *ngIf="transaction.isReconciled" color="primary" 
                         matTooltip="{{ 'TRANSACTIONS.RECONCILED' | translate }}">
                  check_circle
                </mat-icon>
                <mat-icon *ngIf="transaction.isRecurring" color="accent" 
                         matTooltip="{{ 'TRANSACTIONS.RECURRING' | translate }}">
                  repeat
                </mat-icon>
                <mat-icon *ngIf="transaction.uploadedFile" color="primary" 
                         matTooltip="{{ 'TRANSACTIONS.HAS_ATTACHMENT' | translate }}">
                  attach_file
                </mat-icon>
              </div>
            </mat-cell>
          </ng-container>
          
          <ng-container matColumnDef="actions">
            <mat-header-cell *matHeaderCellDef>{{ 'COMMON.ACTIONS' | translate }}</mat-header-cell>
            <mat-cell *matCellDef="let transaction">
              <button mat-icon-button [matMenuTriggerFor]="actionMenu" 
                      (click)="$event.stopPropagation()">
                <mat-icon>more_vert</mat-icon>
              </button>
              <mat-menu #actionMenu="matMenu">
                <button mat-menu-item (click)="editTransaction(transaction)">
                  <mat-icon>edit</mat-icon>
                  {{ 'COMMON.EDIT' | translate }}
                </button>
                <button mat-menu-item (click)="duplicateTransaction(transaction)">
                  <mat-icon>content_copy</mat-icon>
                  {{ 'TRANSACTIONS.DUPLICATE' | translate }}
                </button>
                <button mat-menu-item (click)="uploadReceipt(transaction)">
                  <mat-icon>attach_file</mat-icon>
                  {{ 'TRANSACTIONS.UPLOAD_RECEIPT' | translate }}
                </button>
                <mat-divider></mat-divider>
                <button mat-menu-item (click)="toggleReconciled(transaction)">
                  <mat-icon>{{ transaction.isReconciled ? 'radio_button_unchecked' : 'check_circle' }}</mat-icon>
                  {{ transaction.isReconciled ? 
                      ('TRANSACTIONS.MARK_UNRECONCILED' | translate) : 
                      ('TRANSACTIONS.MARK_RECONCILED' | translate) }}
                </button>
                <mat-divider></mat-divider>
                <button mat-menu-item (click)="deleteTransaction(transaction)" class="delete-action">
                  <mat-icon>delete</mat-icon>
                  {{ 'COMMON.DELETE' | translate }}
                </button>
              </mat-menu>
            </mat-cell>
          </ng-container>
          
          <mat-header-row *matHeaderRowDef="displayedColumns"></mat-header-row>
          <mat-row *matRowDef="let transaction; columns: displayedColumns;" 
                   (click)="viewTransactionDetails(transaction)">
          </mat-row>
        </mat-table>
        
        <!-- No data state -->
        <div class="no-data" *ngIf="dataSource.data.length === 0 && !loading">
          <mat-icon>account_balance_wallet</mat-icon>
          <h3>{{ 'TRANSACTIONS.NO_DATA' | translate }}</h3>
          <p>{{ 'TRANSACTIONS.NO_DATA_DESCRIPTION' | translate }}</p>
          <button mat-raised-button color="primary" (click)="openCreateDialog()">
            <mat-icon>add</mat-icon>
            {{ 'TRANSACTIONS.ADD_FIRST' | translate }}
          </button>
        </div>
        
        <!-- Pagination -->
        <mat-paginator 
          [pageSizeOptions]="[10, 20, 50, 100]" 
          [showFirstLastButtons]="true"
          [pageSize]="pageSize"
          [length]="totalCount"
          (page)="onPageChange($event)">
        </mat-paginator>
      </mat-card>
    </div>
  `,
  styles: [`
    .transaction-container {
      padding: 20px;
      max-width: 1400px;
      margin: 0 auto;
    }
    
    .filter-card {
      margin-bottom: 20px;
    }
    
    .filter-card mat-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    
    .header-actions {
      margin-left: auto;
    }
    
    .filter-row {
      display: flex;
      gap: 16px;
      align-items: center;
      margin-bottom: 16px;
      flex-wrap: wrap;
    }
    
    .filter-row:last-child {
      margin-bottom: 0;
    }
    
    .search-field {
      flex: 2;
      min-width: 200px;
    }
    
    .filter-row mat-form-field {
      flex: 1;
      min-width: 150px;
    }
    
    .transactions-table-card {
      overflow: hidden;
    }
    
    .table-actions {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px;
      border-bottom: 1px solid #e0e0e0;
    }
    
    .bulk-actions {
      display: flex;
      gap: 12px;
      align-items: center;
    }
    
    .selection-info {
      font-size: 14px;
      color: #666;
      margin-right: 16px;
    }
    
    .table-info {
      display: flex;
      align-items: center;
      gap: 12px;
    }
    
    .total-info {
      font-size: 14px;
      color: #666;
    }
    
    .transactions-table {
      width: 100%;
    }
    
    .date-cell {
      display: flex;
      flex-direction: column;
    }
    
    .date {
      font-weight: 500;
    }
    
    .time {
      color: #666;
    }
    
    .description-cell {
      max-width: 300px;
    }
    
    .transaction-description {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    
    .description-text {
      font-weight: 500;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    
    .transaction-meta mat-chip-listbox {
      --mdc-chip-container-height: 24px;
    }
    
    .merchant-chip {
      background: #e3f2fd;
      color: #1976d2;
    }
    
    .reference-chip {
      background: #f3e5f5;
      color: #7b1fa2;
    }
    
    .category-cell, .category-chip {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .no-category {
      display: flex;
      align-items: center;
      gap: 4px;
      color: #f57c00;
      font-size: 12px;
    }
    
    .amount-cell {
      text-align: right;
    }
    
    .amount-display {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
    }
    
    .gross-amount {
      font-weight: 600;
      font-size: 16px;
    }
    
    .amount-display.income .gross-amount {
      color: #4caf50;
    }
    
    .amount-display.expense .gross-amount {
      color: #f44336;
    }
    
    .vat-info {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 2px;
      margin-top: 4px;
    }
    
    .vat-info small {
      font-size: 11px;
      color: #666;
    }
    
    .account-cell {
      display: flex;
      align-items: center;
      gap: 4px;
    }
    
    .status-indicators {
      display: flex;
      gap: 4px;
    }
    
    .category-option {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .income-icon {
      color: #4caf50;
    }
    
    .expense-icon {
      color: #f44336;
    }
    
    .no-data {
      text-align: center;
      padding: 60px 20px;
      color: #666;
    }
    
    .no-data mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
      opacity: 0.5;
    }
    
    .delete-action {
      color: #f44336;
    }
    
    mat-row:hover {
      background-color: #f5f5f5;
      cursor: pointer;
    }
    
    @media (max-width: 768px) {
      .filter-row {
        flex-direction: column;
        align-items: stretch;
      }
      
      .search-field,
      .filter-row mat-form-field {
        flex: none;
        width: 100%;
      }
      
      .table-actions {
        flex-direction: column;
        gap: 12px;
        align-items: stretch;
      }
      
      .bulk-actions,
      .table-info {
        justify-content: center;
      }
    }
  `]
})
export class TransactionListComponent implements OnInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  // Input properties required by parent component
  @Input() pageSize: number = 25;
  @Input() allowBulkOperations: boolean = true;
  
  // Output events
  @Output() transactionSelected = new EventEmitter<Transaction>();
  @Output() bulkSelectionChanged = new EventEmitter<Transaction[]>();
  @Output() transactionEdit = new EventEmitter<Transaction>();
  @Output() transactionDelete = new EventEmitter<Transaction>();

  displayedColumns = ['select', 'date', 'description', 'category', 'amount', 'account', 'status', 'actions'];
  dataSource = new MatTableDataSource<Transaction>();
  selection = new SelectionModel<Transaction>(true, []);
  
  // Filter properties
  searchQuery = '';
  selectedCategoryId = '';
  selectedTransactionType = '';
  dateFrom?: Date;
  dateTo?: Date;
  minAmount?: number;
  maxAmount?: number;
  
  // Data properties
  categories: Category[] = [];
  categoryGroups: { label: string; categories: Category[] }[] = [];
  totalCount = 0;
  // pageSize removed - using @Input pageSize instead
  currentPage = 0;
  loading = false;
  
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  constructor(
    private transactionService: TransactionService,
    private categoryService: CategoryService,
    private fileService: FileService,
    private loadingService: LoadingService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.setupSearchDebounce();
    this.loadCategories();
    this.loadTransactions();
    
    // Listen to loading state
    this.loadingService.loading$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loading => this.loading = loading);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearchDebounce(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.applyFilters();
    });
  }

  loadCategories(): void {
    this.categoryService.getCategoryHierarchy().subscribe({
      next: (hierarchies) => {
        this.categoryGroups = hierarchies.map(h => ({
          label: this.translate.instant(`CATEGORY_TYPE.${h.categoryType.toUpperCase()}`),
          categories: this.flattenCategories(h.categories)
        }));
        
        this.categories = this.categoryGroups.flatMap(g => g.categories);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.showErrorMessage('ERRORS.LOADING_CATEGORIES');
      }
    });
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

  loadTransactions(): void {
    const params: TransactionQueryParameters = {
      page: this.currentPage + 1,
      pageSize: this.pageSize,
      sortBy: this.sort?.active || 'transactionDate',
      sortDirection: this.sort?.direction?.toLowerCase() as 'asc' | 'desc' || 'desc',
      searchTerm: this.searchQuery || undefined,
      categoryId: this.selectedCategoryId || undefined,
      transactionType: this.selectedTransactionType as TransactionType || undefined,
      startDate: this.dateFrom,
      endDate: this.dateTo,
      minAmount: this.minAmount,
      maxAmount: this.maxAmount
    };

    this.transactionService.getTransactions(params).subscribe({
      next: (result: PagedResult<Transaction>) => {
        this.dataSource.data = result.items;
        this.totalCount = result.totalCount;
        this.selection.clear();
      },
      error: (error) => {
        console.error('Error loading transactions:', error);
        this.showErrorMessage('ERRORS.LOADING_TRANSACTIONS');
      }
    });
  }

  onSearchChange(): void {
    this.searchSubject.next(this.searchQuery);
  }

  applyFilters(): void {
    this.currentPage = 0;
    this.loadTransactions();
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.selectedCategoryId = '';
    this.selectedTransactionType = '';
    this.dateFrom = undefined;
    this.dateTo = undefined;
    this.minAmount = undefined;
    this.maxAmount = undefined;
    this.applyFilters();
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadTransactions();
  }

  refreshData(): void {
    this.loadTransactions();
  }

  // Selection methods
  isAllSelected(): boolean {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle(): void {
    this.isAllSelected() ?
      this.selection.clear() :
      this.dataSource.data.forEach(row => this.selection.select(row));
  }

  toggleSelection(transaction: Transaction): void {
    this.selection.toggle(transaction);
  }

  getSelectionText(): string {
    const count = this.selection.selected.length;
    return this.translate.instant('TRANSACTIONS.ITEMS_SELECTED', { count });
  }

  // Dialog methods
  openCreateDialog(): void {
    const dialogRef = this.dialog.open(TransactionFormComponent, {
      width: '800px',
      maxWidth: '95vw',
      data: { transaction: null, isEdit: false }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTransactions();
        this.showSuccessMessage('TRANSACTIONS.CREATED_SUCCESSFULLY');
      }
    });
  }

  editTransaction(transaction: Transaction): void {
    const dialogRef = this.dialog.open(TransactionFormComponent, {
      width: '800px',
      maxWidth: '95vw',
      data: { transaction, isEdit: true }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTransactions();
        this.showSuccessMessage('TRANSACTIONS.UPDATED_SUCCESSFULLY');
      }
    });
  }

  openBulkUpdateDialog(): void {
    const dialogRef = this.dialog.open(BulkUpdateDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { 
        transactions: this.selection.selected,
        categories: this.categories
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTransactions();
        this.selection.clear();
        this.showSuccessMessage('TRANSACTIONS.BULK_UPDATED_SUCCESSFULLY');
      }
    });
  }

  uploadReceipt(transaction: Transaction): void {
    const dialogRef = this.dialog.open(PdfUploadComponent, {
      width: '600px',
      maxWidth: '95vw',
      data: { 
        transactionId: transaction.id,
        fileType: 'Receipt'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTransactions();
        this.showSuccessMessage('FILES.UPLOADED_SUCCESSFULLY');
      }
    });
  }

  // Transaction actions
  duplicateTransaction(transaction: Transaction): void {
    this.transactionService.duplicateTransaction(transaction.id).subscribe({
      next: (duplicated) => {
        this.loadTransactions();
        this.showSuccessMessage('TRANSACTIONS.DUPLICATED_SUCCESSFULLY');
      },
      error: (error) => {
        console.error('Error duplicating transaction:', error);
        this.showErrorMessage('ERRORS.DUPLICATING_TRANSACTION');
      }
    });
  }

  toggleReconciled(transaction: Transaction): void {
    const updateData = { isReconciled: !transaction.isReconciled };
    
    this.transactionService.updateTransaction(transaction.id, updateData).subscribe({
      next: () => {
        this.loadTransactions();
        const messageKey = transaction.isReconciled ? 
          'TRANSACTIONS.MARKED_UNRECONCILED' : 'TRANSACTIONS.MARKED_RECONCILED';
        this.showSuccessMessage(messageKey);
      },
      error: (error) => {
        console.error('Error updating reconciliation status:', error);
        this.showErrorMessage('ERRORS.UPDATING_TRANSACTION');
      }
    });
  }

  deleteTransaction(transaction: Transaction): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: this.translate.instant('TRANSACTIONS.DELETE_TITLE'),
        message: this.translate.instant('TRANSACTIONS.DELETE_CONFIRMATION', { 
          description: transaction.description 
        }),
        confirmButtonText: this.translate.instant('COMMON.DELETE'),
        confirmButtonColor: 'warn'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.transactionService.deleteTransaction(transaction.id).subscribe({
          next: () => {
            this.loadTransactions();
            this.showSuccessMessage('TRANSACTIONS.DELETED_SUCCESSFULLY');
          },
          error: (error) => {
            console.error('Error deleting transaction:', error);
            this.showErrorMessage('ERRORS.DELETING_TRANSACTION');
          }
        });
      }
    });
  }

  bulkDeleteTransactions(): void {
    const selectedIds = this.selection.selected.map(t => t.id);
    
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: this.translate.instant('TRANSACTIONS.BULK_DELETE_TITLE'),
        message: this.translate.instant('TRANSACTIONS.BULK_DELETE_CONFIRMATION', { 
          count: selectedIds.length 
        }),
        confirmButtonText: this.translate.instant('COMMON.DELETE'),
        confirmButtonColor: 'warn'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.transactionService.bulkDeleteTransactions(selectedIds).subscribe({
          next: (deletedCount) => {
            this.loadTransactions();
            this.selection.clear();
            this.showSuccessMessage('TRANSACTIONS.BULK_DELETED_SUCCESSFULLY', { count: deletedCount });
          },
          error: (error) => {
            console.error('Error bulk deleting transactions:', error);
            this.showErrorMessage('ERRORS.BULK_DELETING_TRANSACTIONS');
          }
        });
      }
    });
  }

  exportTransactions(): void {
    const params: TransactionQueryParameters = {
      searchTerm: this.searchQuery || undefined,
      categoryId: this.selectedCategoryId || undefined,
      transactionType: this.selectedTransactionType as TransactionType || undefined,
      startDate: this.dateFrom,
      endDate: this.dateTo,
      minAmount: this.minAmount,
      maxAmount: this.maxAmount
    };

    this.transactionService.exportTransactions(params).subscribe({
      next: (blob: any) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `transactions_${new Date().toISOString().split('T')[0]}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.showSuccessMessage('TRANSACTIONS.EXPORTED_SUCCESSFULLY');
      },
      error: (error: any) => {
        console.error('Error exporting transactions:', error);
        this.showErrorMessage('ERRORS.EXPORTING_TRANSACTIONS');
      }
    });
  }

  viewTransactionDetails(transaction: Transaction): void {
    // This could open a detailed view dialog or navigate to a details page
    console.log('View transaction details:', transaction);
  }

  private showSuccessMessage(messageKey: string, params?: any): void {
    const message = this.translate.instant(messageKey, params);
    this.snackBar.open(message, '', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  private showErrorMessage(messageKey: string, params?: any): void {
    const message = this.translate.instant(messageKey, params);
    this.snackBar.open(message, this.translate.instant('COMMON.CLOSE'), {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}
