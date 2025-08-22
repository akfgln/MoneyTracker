import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  VATSummary,
  ReportHistoryItem,
  ReportParameters,
  ExportOptions
} from '../models/dashboard.types';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly apiUrl = `${environment.apiUrl}/api/reports`;
  
  private reportHistorySubject = new BehaviorSubject<ReportHistoryItem[]>([]);
  public reportHistory$ = this.reportHistorySubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadReportHistory();
  }

  /**
   * Generate monthly financial report
   */
  generateMonthlyReport(
    year: number,
    month: number,
    format: 'PDF' | 'CSV' | 'EXCEL' = 'PDF'
  ): Observable<Blob> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString())
      .set('format', format)
      .set('locale', 'de-DE');

    return this.http.get(`${this.apiUrl}/monthly`, {
      params,
      responseType: 'blob'
    }).pipe(
      map(blob => {
        this.addToHistory({
          name: `Monatsbericht ${this.getGermanMonthName(month)} ${year}`,
          type: 'monthly',
          format,
          parameters: { year, month }
        });
        return blob;
      })
    );
  }

  /**
   * Generate yearly financial report
   */
  generateYearlyReport(
    year: number,
    format: 'PDF' | 'CSV' | 'EXCEL' = 'PDF'
  ): Observable<Blob> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('format', format)
      .set('locale', 'de-DE');

    return this.http.get(`${this.apiUrl}/yearly`, {
      params,
      responseType: 'blob'
    }).pipe(
      map(blob => {
        this.addToHistory({
          name: `Jahresbericht ${year}`,
          type: 'yearly',
          format,
          parameters: { year }
        });
        return blob;
      })
    );
  }

  /**
   * Generate VAT report (Umsatzsteuervoranmeldung)
   */
  generateVatReport(
    startDate: Date,
    endDate: Date,
    format: 'PDF' | 'CSV' = 'PDF',
    includeDetails: boolean = true
  ): Observable<Blob> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString())
      .set('format', format)
      .set('includeDetails', includeDetails.toString())
      .set('locale', 'de-DE');

    return this.http.get(`${this.apiUrl}/vat`, {
      params,
      responseType: 'blob'
    }).pipe(
      map(blob => {
        this.addToHistory({
          name: `MwSt.-Bericht ${this.formatGermanDate(startDate)} - ${this.formatGermanDate(endDate)}`,
          type: 'vat',
          format,
          parameters: { startDate, endDate, includeVat: true }
        });
        return blob;
      })
    );
  }

  /**
   * Generate category analysis report
   */
  generateCategoryReport(
    categoryId?: string,
    year?: number,
    format: 'PDF' | 'CSV' | 'EXCEL' = 'PDF',
    period: 'month' | 'quarter' | 'year' | 'custom' = 'year'
  ): Observable<Blob> {
    let params = new HttpParams()
      .set('format', format)
      .set('period', period)
      .set('locale', 'de-DE');

    if (categoryId) params = params.set('categoryId', categoryId);
    if (year) params = params.set('year', year.toString());

    return this.http.get(`${this.apiUrl}/category`, {
      params,
      responseType: 'blob'
    }).pipe(
      map(blob => {
        const categoryName = categoryId ? 'Kategorie-spezifisch' : 'Alle Kategorien';
        this.addToHistory({
          name: `Kategorienbericht ${categoryName} ${year || new Date().getFullYear()}`,
          type: 'category',
          format,
          parameters: { categoryId, year, groupBy: 'category' }
        });
        return blob;
      })
    );
  }

  /**
   * Generate custom report with advanced filters
   */
  generateCustomReport(
    parameters: ReportParameters,
    format: 'PDF' | 'CSV' | 'EXCEL' = 'PDF'
  ): Observable<Blob> {
    let params = new HttpParams()
      .set('format', format)
      .set('locale', 'de-DE');

    // Add all parameters
    Object.entries(parameters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        if (value instanceof Date) {
          params = params.set(key, value.toISOString());
        } else {
          params = params.set(key, value.toString());
        }
      }
    });

    return this.http.get(`${this.apiUrl}/custom`, {
      params,
      responseType: 'blob'
    }).pipe(
      map(blob => {
        this.addToHistory({
          name: `Benutzerdefinierter Bericht ${this.formatGermanDate(new Date())}`,
          type: 'custom',
          format,
          parameters
        });
        return blob;
      })
    );
  }

  /**
   * Get VAT summary for preview
   */
  getVatSummary(startDate: Date, endDate: Date): Observable<VATSummary> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<VATSummary>(`${this.apiUrl}/vat-summary`, { params })
      .pipe(
        map(summary => ({
          ...summary,
          startDate: new Date(summary.startDate),
          endDate: new Date(summary.endDate),
          vatTransactions: summary.vatTransactions?.map(tx => ({
            ...tx,
            date: new Date(tx.date)
          })) || []
        }))
      );
  }

  /**
   * Generate budget performance report
   */
  generateBudgetReport(
    year: number,
    format: 'PDF' | 'CSV' | 'EXCEL' = 'PDF'
  ): Observable<Blob> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('format', format)
      .set('locale', 'de-DE');

    return this.http.get(`${this.apiUrl}/budget`, {
      params,
      responseType: 'blob'
    }).pipe(
      map(blob => {
        this.addToHistory({
          name: `Budget-Bericht ${year}`,
          type: 'custom',
          format,
          parameters: { year, groupBy: 'category' }
        });
        return blob;
      })
    );
  }

  /**
   * Generate profit & loss report (German: Gewinn- und Verlustrechnung)
   */
  generateProfitLossReport(
    startDate: Date,
    endDate: Date,
    format: 'PDF' | 'CSV' | 'EXCEL' = 'PDF'
  ): Observable<Blob> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString())
      .set('format', format)
      .set('locale', 'de-DE');

    return this.http.get(`${this.apiUrl}/profit-loss`, {
      params,
      responseType: 'blob'
    }).pipe(
      map(blob => {
        this.addToHistory({
          name: `GuV ${this.formatGermanDate(startDate)} - ${this.formatGermanDate(endDate)}`,
          type: 'custom',
          format,
          parameters: { startDate, endDate }
        });
        return blob;
      })
    );
  }

  /**
   * Generate cash flow report
   */
  generateCashFlowReport(
    year: number,
    format: 'PDF' | 'CSV' | 'EXCEL' = 'PDF'
  ): Observable<Blob> {
    const params = new HttpParams()
      .set('year', year.toString())
      .set('format', format)
      .set('locale', 'de-DE');

    return this.http.get(`${this.apiUrl}/cash-flow`, {
      params,
      responseType: 'blob'
    }).pipe(
      map(blob => {
        this.addToHistory({
          name: `Cashflow-Bericht ${year}`,
          type: 'custom',
          format,
          parameters: { year }
        });
        return blob;
      })
    );
  }

  /**
   * Get report templates
   */
  getReportTemplates(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/templates`);
  }

  /**
   * Load report history
   */
  loadReportHistory(): void {
    this.http.get<ReportHistoryItem[]>(`${this.apiUrl}/history`)
      .subscribe(history => {
        const parsedHistory = history.map(item => ({
          ...item,
          generatedDate: new Date(item.generatedDate),
          parameters: {
            ...item.parameters,
            startDate: item.parameters.startDate ? new Date(item.parameters.startDate) : undefined,
            endDate: item.parameters.endDate ? new Date(item.parameters.endDate) : undefined
          }
        }));
        this.reportHistorySubject.next(parsedHistory);
      });
  }

  /**
   * Download report by ID
   */
  downloadReport(reportId: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${reportId}/download`, {
      responseType: 'blob'
    });
  }

  /**
   * Delete report
   */
  deleteReport(reportId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${reportId}`)
      .pipe(
        map(() => {
          // Remove from local state
          const currentHistory = this.reportHistorySubject.value;
          const updatedHistory = currentHistory.filter(item => item.id !== reportId);
          this.reportHistorySubject.next(updatedHistory);
        })
      );
  }

  /**
   * Get available years for reporting
   */
  getAvailableYears(): Observable<number[]> {
    return this.http.get<number[]>(`${this.apiUrl}/available-years`);
  }

  /**
   * Get report statistics
   */
  getReportStatistics(): Observable<{
    totalReports: number;
    reportsByType: { [key: string]: number };
    reportsByFormat: { [key: string]: number };
    totalSize: number;
  }> {
    return this.http.get<{
      totalReports: number;
      reportsByType: { [key: string]: number };
      reportsByFormat: { [key: string]: number };
      totalSize: number;
    }>(`${this.apiUrl}/statistics`);
  }

  /**
   * Schedule automated report
   */
  scheduleReport(
    reportType: 'monthly' | 'yearly' | 'vat',
    parameters: ReportParameters,
    cronExpression: string,
    emailRecipients: string[]
  ): Observable<{ scheduleId: string }> {
    return this.http.post<{ scheduleId: string }>(`${this.apiUrl}/schedule`, {
      reportType,
      parameters,
      cronExpression,
      emailRecipients
    });
  }

  /**
   * Add report to history (local)
   */
  private addToHistory(reportData: {
    name: string;
    type: ReportHistoryItem['type'];
    format: 'PDF' | 'CSV' | 'EXCEL';
    parameters: ReportParameters;
  }): void {
    const newReport: ReportHistoryItem = {
      id: this.generateReportId(),
      ...reportData,
      generatedDate: new Date(),
      fileSize: 0, // Will be updated by backend
      downloadUrl: '' // Will be updated by backend
    };

    const currentHistory = this.reportHistorySubject.value;
    this.reportHistorySubject.next([newReport, ...currentHistory]);
  }

  /**
   * Generate unique report ID
   */
  private generateReportId(): string {
    return `report_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Get German month name
   */
  private getGermanMonthName(month: number): string {
    const months = [
      'Januar', 'Februar', 'März', 'April', 'Mai', 'Juni',
      'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'
    ];
    return months[month - 1] || 'Unbekannt';
  }

  /**
   * Format date in German format
   */
  private formatGermanDate(date: Date): string {
    return new Intl.DateTimeFormat('de-DE', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(date);
  }

  /**
   * Download and save blob file
   */
  downloadBlob(
    blob: Blob,
    filename: string,
    format: 'PDF' | 'CSV' | 'EXCEL'
  ): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    
    const extension = format === 'EXCEL' ? 'xlsx' : format.toLowerCase();
    link.download = `${filename}.${extension}`;
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    // Clean up
    window.URL.revokeObjectURL(url);
  }

  /**
   * Format file size for display
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}