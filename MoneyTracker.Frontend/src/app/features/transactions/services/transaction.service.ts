import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, map } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GermanFormatService } from './german-format.service';

export interface Transaction {
  id: string;
  amount: number;
  description: string;
  date: Date;
  categoryId?: string;
  category?: Category;
  vatRate?: number;
  vatAmount?: number;
  receiptUrl?: string;
  bankStatementUrl?: string;
  notes?: string;
  tags?: string[];
  isRecurring?: boolean;
  recurringPattern?: RecurringPattern;
  createdAt: Date;
  updatedAt: Date;
  userId: string;
}

export interface Category {
  id: string;
  name: string;
  parentId?: string;
  color?: string;
  icon?: string;
  children?: Category[];
  level: number;
}

export interface RecurringPattern {
  frequency: 'daily' | 'weekly' | 'monthly' | 'yearly';
  interval: number;
  endDate?: Date;
  occurrences?: number;
}

export interface TransactionFilter {
  startDate?: Date;
  endDate?: Date;
  categoryIds?: string[];
  minAmount?: number;
  maxAmount?: number;
  search?: string;
  tags?: string[];
  hasReceipt?: boolean;
  vatRateRange?: { min: number; max: number };
}

export interface TransactionSummary {
  totalTransactions: number;
  totalAmount: number;
  totalVAT: number;
  averageAmount: number;
  categorySummary: CategorySummary[];
  monthlyTrend: MonthlyTrend[];
}

export interface CategorySummary {
  categoryId: string;
  categoryName: string;
  amount: number;
  percentage: number;
  transactionCount: number;
}

export interface MonthlyTrend {
  month: string;
  amount: number;
  transactionCount: number;
}

export interface BulkUpdateData {
  transactionIds: string[];
  updates: Partial<Transaction>;
}

export interface TransactionPagedResult {
  transactions: Transaction[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private readonly apiUrl = `${environment.apiUrl}/api/transactions`;
  private readonly categoriesUrl = `${environment.apiUrl}/api/categories`;
  
  // State management
  private transactionsSubject = new BehaviorSubject<Transaction[]>([]);
  private categoriesSubject = new BehaviorSubject<Category[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  
  public transactions$ = this.transactionsSubject.asObservable();
  public categories$ = this.categoriesSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();
  
  constructor(
    private http: HttpClient,
    private germanFormatService: GermanFormatService
  ) {
    this.loadCategories();
  }
  
  
  /**
   * Get transactions with filtering and pagination
   */
  getTransactions(
    page: number = 1,
    pageSize: number = 50,
    filter?: TransactionFilter
  ): Observable<TransactionPagedResult> {
    this.loadingSubject.next(true);
    
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (filter) {
      if (filter.startDate) {
        params = params.set('startDate', filter.startDate.toISOString());
      }
      if (filter.endDate) {
        params = params.set('endDate', filter.endDate.toISOString());
      }
      if (filter.categoryIds && filter.categoryIds.length > 0) {
        params = params.set('categoryIds', filter.categoryIds.join(','));
      }
      if (filter.minAmount !== undefined) {
        params = params.set('minAmount', filter.minAmount.toString());
      }
      if (filter.maxAmount !== undefined) {
        params = params.set('maxAmount', filter.maxAmount.toString());
      }
      if (filter.search) {
        params = params.set('search', filter.search);
      }
      if (filter.tags && filter.tags.length > 0) {
        params = params.set('tags', filter.tags.join(','));
      }
      if (filter.hasReceipt !== undefined) {
        params = params.set('hasReceipt', filter.hasReceipt.toString());
      }
    }
    
    return this.http.get<TransactionPagedResult>(this.apiUrl, { params })
      .pipe(
        map(result => {
          // Parse dates
          result.transactions = result.transactions.map(t => this.parseTransactionDates(t));
          this.transactionsSubject.next(result.transactions);
          this.loadingSubject.next(false);
          return result;
        })
      );
  }
  
  /**
   * Get a single transaction by ID
   */
  getTransaction(id: string): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.apiUrl}/${id}`)
      .pipe(map(t => this.parseTransactionDates(t)));
  }
  
  /**
   * Create a new transaction
   */
  createTransaction(transaction: Omit<Transaction, 'id' | 'createdAt' | 'updatedAt' | 'userId'>): Observable<Transaction> {
    return this.http.post<Transaction>(this.apiUrl, transaction)
      .pipe(map(t => this.parseTransactionDates(t)));
  }
  
  /**
   * Update an existing transaction
   */
  updateTransaction(id: string, transaction: Partial<Transaction>): Observable<Transaction> {
    return this.http.put<Transaction>(`${this.apiUrl}/${id}`, transaction)
      .pipe(map(t => this.parseTransactionDates(t)));
  }
  
  /**
   * Delete a transaction
   */
  deleteTransaction(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
  
  /**
   * Bulk update transactions
   */
  bulkUpdateTransactions(bulkUpdate: BulkUpdateData): Observable<Transaction[]> {
    return this.http.patch<Transaction[]>(`${this.apiUrl}/bulk`, bulkUpdate)
      .pipe(
        map(transactions => transactions.map(t => this.parseTransactionDates(t)))
      );
  }
  
  /**
   * Bulk delete transactions
   */
  bulkDeleteTransactions(transactionIds: string[]): Observable<void> {
    return this.http.request<void>('DELETE', this.apiUrl, {
      body: { transactionIds }
    });
  }
  
  // Category Operations
  
  /**
   * Load all categories
   */
  loadCategories(): void {
    this.http.get<Category[]>(this.categoriesUrl)
      .subscribe(categories => {
        this.categoriesSubject.next(categories);
      });
  }
  
  /**
   * Get hierarchical category tree
   */
  getCategoryTree(): Observable<Category[]> {
    return this.categories$.pipe(
      map(categories => this.buildCategoryTree(categories))
    );
  }
  
  /**
   * Get category by ID
   */
  getCategory(id: string): Observable<Category | undefined> {
    return this.categories$.pipe(
      map(categories => categories.find(c => c.id === id))
    );
  }
  
  // Analytics and Reporting
  
  /**
   * Get transaction summary for a date range
   */
  getTransactionSummary(
    startDate: Date,
    endDate: Date,
    categoryIds?: string[]
  ): Observable<TransactionSummary> {
    let params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    
    if (categoryIds && categoryIds.length > 0) {
      params = params.set('categoryIds', categoryIds.join(','));
    }
    
    return this.http.get<TransactionSummary>(`${this.apiUrl}/summary`, { params });
  }
  
  /**
   * Get monthly spending trends
   */
  getMonthlyTrends(
    months: number = 12,
    categoryIds?: string[]
  ): Observable<MonthlyTrend[]> {
    let params = new HttpParams().set('months', months.toString());
    
    if (categoryIds && categoryIds.length > 0) {
      params = params.set('categoryIds', categoryIds.join(','));
    }
    
    return this.http.get<MonthlyTrend[]>(`${this.apiUrl}/trends/monthly`, { params });
  }
  
  /**
   * Get category spending analysis
   */
  getCategoryAnalysis(
    startDate: Date,
    endDate: Date
  ): Observable<CategorySummary[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    
    return this.http.get<CategorySummary[]>(`${this.apiUrl}/analysis/categories`, { params });
  }
  
  // Utility Methods
  
  /**
   * Calculate VAT for a transaction
   */
  calculateVAT(amount: number, vatRate: number): {
    netAmount: number;
    vatAmount: number;
    grossAmount: number;
  } {
    const netAmount = amount;
    const vatAmount = netAmount * vatRate;
    const grossAmount = netAmount + vatAmount;
    
    return { netAmount, vatAmount, grossAmount };
  }
  
  /**
   * Format transaction for display
   */
  formatTransactionForDisplay(transaction: Transaction): {
    amount: string;
    date: string;
    vatInfo?: {
      rate: string;
      amount: string;
      vatAmount: string;
      totalAmount: string;
    };
  } {
    const result = {
      amount: this.germanFormatService.formatCurrency(transaction.amount),
      date: this.germanFormatService.formatDate(transaction.date)
    };
    
    if (transaction.vatRate && transaction.vatRate > 0) {
      return {
        ...result,
        vatInfo: this.germanFormatService.formatVAT(transaction.vatRate, transaction.amount)
      };
    }
    
    return result;
  }
  
  /**
   * Export transactions to CSV
   */
  exportToCSV(transactions: Transaction[]): string {
    const headers = [
      'Datum',
      'Beschreibung',
      'Betrag',
      'Kategorie',
      'MwSt.-Satz',
      'MwSt.-Betrag',
      'Notizen',
      'Tags'
    ];
    
    const rows = transactions.map(t => [
      this.germanFormatService.formatDate(t.date),
      t.description,
      this.germanFormatService.formatCurrency(t.amount),
      t.category?.name || '',
      t.vatRate ? this.germanFormatService.formatPercentage(t.vatRate) : '',
      t.vatAmount ? this.germanFormatService.formatCurrency(t.vatAmount) : '',
      t.notes || '',
      t.tags?.join('; ') || ''
    ]);
    
    const csvContent = [headers, ...rows]
      .map(row => row.map(field => `"${field}"`).join(','))
      .join('\n');
    
    return csvContent;
  }
  
  /**
   * Download CSV export
   */
  downloadCSV(transactions: Transaction[], filename: string = 'transaktionen.csv'): void {
    const csvContent = this.exportToCSV(transactions);
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    
    if (link.download !== undefined) {
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', filename);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }
  
  // Smart categorization
  
  /**
   * Get category suggestions for a transaction
   */
  getCategorySuggestions(description: string, amount: number): Observable<Category[]> {
    const params = new HttpParams()
      .set('description', description)
      .set('amount', amount.toString());
    
    return this.http.get<Category[]>(`${this.apiUrl}/suggestions/categories`, { params });
  }
  
  /**
   * Get similar transactions
   */
  getSimilarTransactions(transactionId: string): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/${transactionId}/similar`)
      .pipe(
        map(transactions => transactions.map(t => this.parseTransactionDates(t)))
      );
  }
  
  // Private helper methods
  
  private parseTransactionDates(transaction: Transaction): Transaction {
    return {
      ...transaction,
      date: new Date(transaction.date),
      createdAt: new Date(transaction.createdAt),
      updatedAt: new Date(transaction.updatedAt)
    };
  }
  
  private buildCategoryTree(categories: Category[]): Category[] {
    const categoryMap = new Map<string, Category>();
    const rootCategories: Category[] = [];
    
    // First pass: create all categories with children arrays
    categories.forEach(category => {
      categoryMap.set(category.id, { ...category, children: [] });
    });
    
    // Second pass: build the tree structure
    categories.forEach(category => {
      const categoryWithChildren = categoryMap.get(category.id)!;
      
      if (category.parentId) {
        const parent = categoryMap.get(category.parentId);
        if (parent) {
          parent.children!.push(categoryWithChildren);
        }
      } else {
        rootCategories.push(categoryWithChildren);
      }
    });
    
    return rootCategories;
  }
  
  // Common VAT rates in Germany
  getCommonVATRates(): { label: string; value: number }[] {
    return [
      { label: '0%', value: 0.00 },
      { label: '7% (ermäßigt)', value: 0.07 },
      { label: '19% (Standard)', value: 0.19 }
    ];
  }
  
  // Transaction status helpers
  getTransactionStatus(transaction: Transaction): 'complete' | 'missing-receipt' | 'needs-review' {
    if (!transaction.receiptUrl && transaction.amount > 100) {
      return 'missing-receipt';
    }
    
    if (!transaction.categoryId || !transaction.notes) {
      return 'needs-review';
    }
    
    return 'complete';
  }
}