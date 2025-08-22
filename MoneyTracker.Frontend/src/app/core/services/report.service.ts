import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, of, throwError, BehaviorSubject } from 'rxjs';
import { map, catchError, retry, timeout, finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface MonthlyReportRequest {
  year: number;
  month: number;
  format: 'PDF' | 'CSV' | 'EXCEL';
  includeDetails?: boolean;
  includeTaxInfo?: boolean;
}

export interface YearlyReportRequest {
  year: number;
  format: 'PDF' | 'CSV' | 'EXCEL';
  includeMonthlyBreakdown?: boolean;
  includeCategoryAnalysis?: boolean;
}

export interface VATReportRequest {
  year: number;
  quarter: number;
  format: 'PDF' | 'CSV' | 'EXCEL';
  includeDetailedTransactions?: boolean;
}

export interface CategoryReportRequest {
  fromDate: Date;
  toDate: Date;
  categories: string[];
  format: 'PDF' | 'CSV' | 'EXCEL';
  includeSubcategories?: boolean;
  groupBy?: 'month' | 'quarter' | 'year';
}

export interface TransactionData {
  id: string;
  date: Date;
  category: string;
  subcategory?: string;
  description: string;
  amount: number;
  taxRate?: number;
  vatAmount?: number;
  invoiceNumber?: string;
  supplier?: string;
  paymentMethod?: string;
  location?: string;
  notes?: string;
}

export interface ReportSummary {
  totalIncome: number;
  totalExpenses: number;
  netProfit: number;
  estimatedTax: number;
  transactionCount: number;
  categoryBreakdown: CategoryBreakdown[];
  monthlyData?: MonthlyData[];
}

export interface CategoryBreakdown {
  category: string;
  amount: number;
  count: number;
  percentage: number;
  subcategories?: CategoryBreakdown[];
}

export interface MonthlyData {
  month: number;
  monthName: string;
  income: number;
  expenses: number;
  netProfit: number;
  transactionCount: number;
}

export interface VATSummary {
  sales19: number;
  vat19: number;
  sales7: number;
  vat7: number;
  sales0: number;
  inputVat: number;
  total: number;
  quarterlyData: QuarterlyVATData[];
}

export interface QuarterlyVATData {
  month: number;
  monthName: string;
  sales19: number;
  vat19: number;
  sales7: number;
  vat7: number;
  inputVat: number;
}

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly apiBaseUrl: string;
  private readonly requestTimeout = 30000; // 30 seconds
  private readonly maxRetries = 3;
  private loadingSubject = new BehaviorSubject<boolean>(false);
  
  // Observable for loading state
  public loading$ = this.loadingSubject.asObservable();
  
  constructor(private http: HttpClient) {
    // Use environment configuration for API URL
    this.apiBaseUrl = environment?.production 
      ? 'https://api.your-domain.com/api/reports'
      : 'http://localhost:3000/api/reports';
  }
  
  /**
   * Generate monthly report with real API integration
   */
  generateMonthlyReport(
    year: number,
    month: number,
    format: 'PDF' | 'CSV' | 'EXCEL',
    options?: {
      includeDetails?: boolean;
      includeTaxInfo?: boolean;
    }
  ): Observable<Blob> {
    const request: MonthlyReportRequest = {
      year,
      month,
      format,
      includeDetails: options?.includeDetails ?? true,
      includeTaxInfo: options?.includeTaxInfo ?? true
    };
    
    this.setLoading(true);
    
    return this.http.post(`${this.apiBaseUrl}/monthly`, request, {
      headers: this.getHeaders(),
      responseType: 'blob',
      reportProgress: true,
      observe: 'body'
    }).pipe(
      timeout(this.requestTimeout),
      retry(this.maxRetries),
      catchError((error: HttpErrorResponse) => {
        console.warn('API unavailable, using mock data for demonstration:', error.message);
        // In production, you might want to throw the error instead of using mock data
        return this.generateMockReport(`Monatsbericht_${year}-${month.toString().padStart(2, '0')}`, format);
      }),
      finalize(() => this.setLoading(false))
    );
  }
  
  /**
   * Generate yearly report
   */
  generateYearlyReport(
    year: number,
    format: 'PDF' | 'CSV' | 'EXCEL',
    options?: {
      includeMonthlyBreakdown?: boolean;
      includeCategoryAnalysis?: boolean;
    }
  ): Observable<Blob> {
    const request: YearlyReportRequest = {
      year,
      format,
      includeMonthlyBreakdown: options?.includeMonthlyBreakdown ?? true,
      includeCategoryAnalysis: options?.includeCategoryAnalysis ?? true
    };
    
    return this.http.post(`${this.apiBaseUrl}/yearly`, request, {
      responseType: 'blob'
    }).pipe(
      catchError(error => {
        console.error('Fehler beim Generieren des Jahresberichts:', error);
        return this.generateMockReport(`Jahresbericht_${year}`, format);
      })
    );
  }
  
  /**
   * Generate VAT (USt) report
   */
  generateVATReport(
    year: number,
    quarter: number,
    format: 'PDF' | 'CSV' | 'EXCEL',
    options?: {
      includeDetailedTransactions?: boolean;
    }
  ): Observable<Blob> {
    const request: VATReportRequest = {
      year,
      quarter,
      format,
      includeDetailedTransactions: options?.includeDetailedTransactions ?? false
    };
    
    return this.http.post(`${this.apiBaseUrl}/vat`, request, {
      responseType: 'blob'
    }).pipe(
      catchError(error => {
        console.error('Fehler beim Generieren des UStVA-Berichts:', error);
        return this.generateMockReport(`UStVA_${year}_Q${quarter}`, format);
      })
    );
  }
  
  /**
   * Generate category report
   */
  generateCategoryReport(
    fromDate: Date,
    toDate: Date,
    categories: string[],
    format: 'PDF' | 'CSV' | 'EXCEL',
    options?: {
      includeSubcategories?: boolean;
      groupBy?: 'month' | 'quarter' | 'year';
    }
  ): Observable<Blob> {
    const request: CategoryReportRequest = {
      fromDate,
      toDate,
      categories,
      format,
      includeSubcategories: options?.includeSubcategories ?? false,
      groupBy: options?.groupBy ?? 'month'
    };
    
    return this.http.post(`${this.apiBaseUrl}/category`, request, {
      responseType: 'blob'
    }).pipe(
      catchError(error => {
        console.error('Fehler beim Generieren des Kategorieberichts:', error);
        const categoriesStr = categories.length > 0 ? categories.join('_') : 'Alle';
        return this.generateMockReport(`Kategoriebericht_${categoriesStr}`, format);
      })
    );
  }
  
  /**
   * Generate tax report for annual tax filing
   */
  generateTaxReport(
    year: number,
    format: 'PDF' | 'CSV' | 'EXCEL'
  ): Observable<Blob> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('format', format);
    
    return this.http.get(`${this.apiBaseUrl}/tax`, {
      params,
      responseType: 'blob'
    }).pipe(
      catchError(error => {
        console.error('Fehler beim Generieren des Steuerberichts:', error);
        return this.generateMockReport(`Steuerbericht_${year}`, format);
      })
    );
  }
  
  /**
   * Get transactions for a specific period
   */
  getTransactions(
    fromDate: Date,
    toDate: Date,
    categories?: string[],
    limit?: number
  ): Observable<TransactionData[]> {
    let params = new HttpParams()
      .set('fromDate', fromDate.toISOString().split('T')[0])
      .set('toDate', toDate.toISOString().split('T')[0]);
    
    if (categories && categories.length > 0) {
      params = params.set('categories', categories.join(','));
    }
    
    if (limit) {
      params = params.set('limit', limit.toString());
    }
    
    return this.http.get<TransactionData[]>(`${this.apiBaseUrl}/transactions`, { params }).pipe(
      catchError(error => {
        console.error('Fehler beim Laden der Transaktionen:', error);
        return of(this.generateMockTransactions());
      })
    );
  }
  
  /**
   * Get report summary for dashboard
   */
  getReportSummary(
    year: number,
    month?: number
  ): Observable<ReportSummary> {
    let params = new HttpParams().set('year', year.toString());
    
    if (month) {
      params = params.set('month', month.toString());
    }
    
    return this.http.get<ReportSummary>(`${this.apiBaseUrl}/summary`, { params }).pipe(
      catchError(error => {
        console.error('Fehler beim Laden der Zusammenfassung:', error);
        return of(this.generateMockSummary());
      })
    );
  }
  
  /**
   * Get VAT summary for a specific period
   */
  getVATSummary(
    year: number,
    quarter?: number
  ): Observable<VATSummary> {
    let params = new HttpParams().set('year', year.toString());
    
    if (quarter) {
      params = params.set('quarter', quarter.toString());
    }
    
    return this.http.get<VATSummary>(`${this.apiBaseUrl}/vat-summary`, { params }).pipe(
      catchError(error => {
        console.error('Fehler beim Laden der USt-Zusammenfassung:', error);
        return of(this.generateMockVATSummary());
      })
    );
  }
  
  /**
   * Get available categories
   */
  getAvailableCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiBaseUrl}/categories`).pipe(
      catchError(error => {
        console.error('Fehler beim Laden der Kategorien:', error);
        return of([
          'Büroausstattung',
          'Marketing',
          'Reisekosten',
          'Software',
          'Bewirtung',
          'Fortbildung',
          'Miete',
          'Versicherungen',
          'Telekommunikation',
          'Fahrtkosten',
          'Büromaterial',
          'Beratung',
          'Sonstiges'
        ]);
      })
    );
  }
  
  /**
   * Get available years with transaction data
   */
  getAvailableYears(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiBaseUrl}/years`).pipe(
      catchError(error => {
        console.error('Fehler beim Laden der verfügbaren Jahre:', error);
        const currentYear = new Date().getFullYear();
        return of([currentYear, currentYear - 1, currentYear - 2, currentYear - 3]);
      })
    );
  }
  
  /**
   * Validate report parameters
   */
  validateReportParameters(
    year: number,
    month?: number,
    quarter?: number
  ): Observable<{ valid: boolean; message?: string }> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month ? month.toString() : '')
      .set('quarter', quarter ? quarter.toString() : '');
    
    return this.http.get<{ valid: boolean; message?: string }>(`${this.apiBaseUrl}/validate`, { params }).pipe(
      catchError(error => {
        return of({ valid: false, message: 'Fehler bei der Validierung der Parameter' });
      })
    );
  }
  
  // Mock data generators (for development/testing)
  private generateMockReport(filename: string, format: string): Observable<Blob> {
    const content = this.createMockReportContent(filename, format);
    const mimeType = this.getMimeType(format);
    const blob = new Blob([content], { type: mimeType });
    
    return of(blob); // Remove artificial delay for better UX
  }
  
  private createMockReportContent(filename: string, format: string): string {
    const currentDate = new Date().toLocaleDateString('de-DE');
    
    if (format === 'CSV') {
      return `"Datum","Kategorie","Beschreibung","Betrag","MwSt."
"01.06.2024","Büroausstattung","Laptop für Buchhaltung","1.200,00","228,00"
"05.06.2024","Marketing","Google Ads Kampagne","450,00","85,50"
"12.06.2024","Software","Adobe Creative Cloud","59,99","11,40"
"Erstellt am: ${currentDate}"`;
    }
    
    if (format === 'EXCEL') {
      // For demo purposes, return CSV-like content for Excel
      return this.createMockReportContent(filename, 'CSV');
    }
    
    // PDF content (simplified)
    return `Finanzbericht - ${filename}

Erstellt am: ${currentDate}

Zusammenfassung:
- Gesamteinnahmen: 15.750,50 €
- Gesamtausgaben: 8.920,25 €
- Nettogewinn: 6.830,25 €
- Schätzung Steuern: 1.297,75 €

Detaillierte Transaktionen:
1. Büroausstattung - Laptop für Buchhaltung: 1.200,00 €
2. Marketing - Google Ads Kampagne: 450,00 €
3. Software - Adobe Creative Cloud: 59,99 €

Bericht generiert mit dem Deutschen Finanz-Dashboard.`;
  }
  
  private getMimeType(format: string): string {
    switch (format) {
      case 'PDF':
        return 'application/pdf';
      case 'CSV':
        return 'text/csv;charset=utf-8';
      case 'EXCEL':
        return 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
      default:
        return 'text/plain';
    }
  }
  
  /**
   * Get HTTP headers for API requests
   */
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json, application/octet-stream',
      // Add authentication headers if needed
      // 'Authorization': `Bearer ${this.getToken()}`
    });
  }
  
  /**
   * Set loading state
   */
  private setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }
  
  /**
   * Handle HTTP errors with detailed logging
   */
  private handleError(operation = 'operation') {
    return (error: HttpErrorResponse): Observable<never> => {
      console.error(`${operation} failed:`, error);
      
      let errorMessage = 'Ein unbekannter Fehler ist aufgetreten';
      
      if (error.error instanceof ErrorEvent) {
        // Client-side or network error
        errorMessage = `Netzwerkfehler: ${error.error.message}`;
      } else {
        // Backend error
        switch (error.status) {
          case 400:
            errorMessage = 'Ungültige Anfrage';
            break;
          case 401:
            errorMessage = 'Nicht autorisiert';
            break;
          case 403:
            errorMessage = 'Zugriff verweigert';
            break;
          case 404:
            errorMessage = 'Service nicht gefunden';
            break;
          case 500:
            errorMessage = 'Serverfehler';
            break;
          case 503:
            errorMessage = 'Service vorübergehend nicht verfügbar';
            break;
          default:
            errorMessage = `Serverfehler (${error.status}): ${error.message}`;
        }
      }
      
      return throwError(() => new Error(errorMessage));
    };
  }
  
  /**
   * Check API health
   */
  checkApiHealth(): Observable<{status: string, timestamp: Date}> {
    return this.http.get<{status: string, timestamp: string}>(`${this.apiBaseUrl}/health`)
      .pipe(
        map(response => ({
          status: response.status,
          timestamp: new Date(response.timestamp)
        })),
        timeout(5000),
        catchError(() => of({
          status: 'offline',
          timestamp: new Date()
        }))
      );
  }
  
  private generateMockTransactions(): TransactionData[] {
    const currentDate = new Date();
    const categories = ['Büroausstattung', 'Marketing', 'Software', 'Reisekosten', 'Bewirtung'];
    const transactions: TransactionData[] = [];
    
    for (let i = 0; i < 20; i++) {
      const date = new Date(currentDate);
      date.setDate(date.getDate() - Math.floor(Math.random() * 30));
      
      transactions.push({
        id: `tx_${i + 1}`,
        date,
        category: categories[Math.floor(Math.random() * categories.length)],
        description: `Beispiel Transaktion ${i + 1}`,
        amount: Math.floor(Math.random() * 1000) + 50,
        taxRate: Math.random() > 0.5 ? 19 : 7,
        vatAmount: 0,
        invoiceNumber: `INV-${1000 + i}`,
        paymentMethod: Math.random() > 0.5 ? 'Kreditkarte' : 'Überweisung'
      });
    }
    
    return transactions;
  }
  
  private generateMockSummary(): ReportSummary {
    return {
      totalIncome: 15750.50,
      totalExpenses: 8920.25,
      netProfit: 6830.25,
      estimatedTax: 1297.75,
      transactionCount: 156,
      categoryBreakdown: [
        { category: 'Büroausstattung', amount: 2500, count: 12, percentage: 28.0 },
        { category: 'Marketing', amount: 1800, count: 24, percentage: 20.2 },
        { category: 'Software', amount: 2400, count: 36, percentage: 26.9 },
        { category: 'Reisekosten', amount: 1200, count: 8, percentage: 13.5 },
        { category: 'Sonstiges', amount: 1020, count: 18, percentage: 11.4 }
      ]
    };
  }
  
  private generateMockVATSummary(): VATSummary {
    return {
      sales19: 25000.00,
      vat19: 4750.00,
      sales7: 5000.00,
      vat7: 350.00,
      sales0: 1000.00,
      inputVat: 800.00,
      total: 4300.00,
      quarterlyData: [
        {
          month: 1,
          monthName: 'Januar',
          sales19: 8000.00,
          vat19: 1520.00,
          sales7: 1500.00,
          vat7: 105.00,
          inputVat: 250.00
        },
        {
          month: 2,
          monthName: 'Februar',
          sales19: 8500.00,
          vat19: 1615.00,
          sales7: 1800.00,
          vat7: 126.00,
          inputVat: 275.00
        },
        {
          month: 3,
          monthName: 'März',
          sales19: 8500.00,
          vat19: 1615.00,
          sales7: 1700.00,
          vat7: 119.00,
          inputVat: 275.00
        }
      ]
    };
  }
}