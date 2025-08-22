import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { TranslateModule } from '@ngx-translate/core';
import { ReportService } from '../../core/services/report.service';
import { ExportService } from '../../core/services/export.service';

interface ReportData {
  id: string;
  date: Date;
  category: string;
  description: string;
  amount: number;
  taxRate?: number;
  vatAmount?: number;
}

interface CategorySummary {
  category: string;
  totalAmount: number;
  count: number;
  percentage: number;
}

@Component({
  selector: 'app-reports',
  template: `
    <div class="reports-container">
      <mat-card class="header-card">
        <mat-card-header>
          <mat-card-title>{{ 'reports.title' | translate }}</mat-card-title>
          <mat-card-subtitle>{{ 'reports.subtitle' | translate }}</mat-card-subtitle>
        </mat-card-header>
      </mat-card>
      
      <mat-tab-group class="reports-tabs">
        <!-- Monthly Reports Tab -->
        <mat-tab label="{{ 'reports.monthlyReports' | translate }}">
          <div class="tab-content">
            <mat-card class="config-card">
              <mat-card-header>
                <mat-card-title>{{ 'reports.monthlyReportsConfig' | translate }}</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="form-row">
                  <mat-form-field>
                    <mat-label>Jahr</mat-label>
                    <mat-select [(value)]="monthlyReportYear">
                      <mat-option *ngFor="let year of availableYears" [value]="year">
                        {{ year }}
                      </mat-option>
                    </mat-select>
                  </mat-form-field>
                  
                  <mat-form-field>
                    <mat-label>Monat</mat-label>
                    <mat-select [(value)]="monthlyReportMonth">
                      <mat-option *ngFor="let month of germanMonths; let i = index" [value]="i + 1">
                        {{ month }}
                      </mat-option>
                    </mat-select>
                  </mat-form-field>
                  
                  <mat-form-field>
                    <mat-label>Format</mat-label>
                    <mat-select [(value)]="monthlyReportFormat">
                      <mat-option value="PDF">PDF</mat-option>
                      <mat-option value="CSV">CSV</mat-option>
                      <mat-option value="EXCEL">Excel</mat-option>
                    </mat-select>
                  </mat-form-field>
                </div>
                
                <div class="action-buttons">
                  <button mat-raised-button color="primary" (click)="generateMonthlyReport()" [disabled]="isGenerating">
                    <mat-icon>description</mat-icon>
                    {{ isGenerating ? 'Generiere...' : 'Bericht generieren' }}
                    <mat-spinner *ngIf="isGenerating" diameter="20"></mat-spinner>
                  </button>
                  <button mat-stroked-button (click)="previewMonthlyReport()">
                    <mat-icon>visibility</mat-icon>
                    Vorschau
                  </button>
                </div>
              </mat-card-content>
            </mat-card>
            
            <!-- Monthly Report Preview -->
            <mat-card *ngIf="monthlyReportData.length > 0" class="preview-card">
              <mat-card-header>
                <mat-card-title>Vorschau: {{ getSelectedMonth() }} {{ monthlyReportYear }}</mat-card-title>
                <div class="summary-chips">
                  <mat-chip-set>
                    <mat-chip color="primary">{{ monthlyReportData.length }} Transaktionen</mat-chip>
                    <mat-chip color="accent">{{ getTotalAmount() | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</mat-chip>
                  </mat-chip-set>
                </div>
              </mat-card-header>
              <mat-card-content>
                <mat-table [dataSource]="monthlyReportData" class="report-table">
                  <ng-container matColumnDef="date">
                    <mat-header-cell *matHeaderCellDef>Datum</mat-header-cell>
                    <mat-cell *matCellDef="let element">{{ element.date | date:'dd.MM.yyyy':'de-DE' }}</mat-cell>
                  </ng-container>
                  
                  <ng-container matColumnDef="category">
                    <mat-header-cell *matHeaderCellDef>Kategorie</mat-header-cell>
                    <mat-cell *matCellDef="let element">{{ element.category }}</mat-cell>
                  </ng-container>
                  
                  <ng-container matColumnDef="description">
                    <mat-header-cell *matHeaderCellDef>Beschreibung</mat-header-cell>
                    <mat-cell *matCellDef="let element">{{ element.description }}</mat-cell>
                  </ng-container>
                  
                  <ng-container matColumnDef="amount">
                    <mat-header-cell *matHeaderCellDef>Betrag</mat-header-cell>
                    <mat-cell *matCellDef="let element">{{ element.amount | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</mat-cell>
                  </ng-container>
                  
                  <mat-header-row *matHeaderRowDef="displayedColumns"></mat-header-row>
                  <mat-row *matRowDef="let row; columns: displayedColumns;"></mat-row>
                </mat-table>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>
        
        <!-- Yearly Reports Tab -->
        <mat-tab label="{{ 'reports.yearlyReports' | translate }}">
          <div class="tab-content">
            <mat-card class="config-card">
              <mat-card-header>
                <mat-card-title>{{ 'reports.yearlyReportsConfig' | translate }}</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="form-row">
                  <mat-form-field>
                    <mat-label>Jahr</mat-label>
                    <mat-select [(value)]="yearlyReportYear">
                      <mat-option *ngFor="let year of availableYears" [value]="year">
                        {{ year }}
                      </mat-option>
                    </mat-select>
                  </mat-form-field>
                  
                  <mat-form-field>
                    <mat-label>Format</mat-label>
                    <mat-select [(value)]="yearlyReportFormat">
                      <mat-option value="PDF">PDF</mat-option>
                      <mat-option value="CSV">CSV</mat-option>
                      <mat-option value="EXCEL">Excel</mat-option>
                    </mat-select>
                  </mat-form-field>
                </div>
                
                <div class="action-buttons">
                  <button mat-raised-button color="primary" (click)="generateYearlyReport()" [disabled]="isGenerating">
                    <mat-icon>assignment</mat-icon>
                    Jahresbericht generieren
                  </button>
                  <button mat-stroked-button (click)="previewYearlyReport()">
                    <mat-icon>visibility</mat-icon>
                    Vorschau
                  </button>
                </div>
              </mat-card-content>
            </mat-card>
            
            <!-- Yearly Summary -->
            <div *ngIf="yearlySummaryData.length > 0" class="summary-grid">
              <mat-card *ngFor="let summary of yearlySummaryData" class="summary-card">
                <mat-card-header>
                  <mat-card-title>{{ summary.category }}</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                  <div class="summary-amount">{{ summary.totalAmount | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</div>
                  <div class="summary-details">
                    {{ summary.count }} Transaktionen ({{ summary.percentage | number:'1.1-1':'de-DE' }}%)
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </div>
        </mat-tab>
        
        <!-- VAT Reports Tab -->
        <mat-tab label="{{ 'reports.vatReports' | translate }}">
          <div class="tab-content">
            <mat-card class="config-card">
              <mat-card-header>
                <mat-card-title>{{ 'reports.vatReportsConfig' | translate }}</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="form-row">
                  <mat-form-field>
                    <mat-label>Jahr</mat-label>
                    <mat-select [(value)]="vatReportYear">
                      <mat-option *ngFor="let year of availableYears" [value]="year">
                        {{ year }}
                      </mat-option>
                    </mat-select>
                  </mat-form-field>
                  
                  <mat-form-field>
                    <mat-label>Quartal</mat-label>
                    <mat-select [(value)]="vatReportQuarter">
                      <mat-option value="1">Q1 (Jan-Mär)</mat-option>
                      <mat-option value="2">Q2 (Apr-Jun)</mat-option>
                      <mat-option value="3">Q3 (Jul-Sep)</mat-option>
                      <mat-option value="4">Q4 (Okt-Dez)</mat-option>
                    </mat-select>
                  </mat-form-field>
                  
                  <mat-form-field>
                    <mat-label>Format</mat-label>
                    <mat-select [(value)]="vatReportFormat">
                      <mat-option value="PDF">PDF</mat-option>
                      <mat-option value="CSV">CSV</mat-option>
                      <mat-option value="EXCEL">Excel</mat-option>
                    </mat-select>
                  </mat-form-field>
                </div>
                
                <div class="action-buttons">
                  <button mat-raised-button color="accent" (click)="generateVATReport()" [disabled]="isGenerating">
                    <mat-icon>receipt_long</mat-icon>
                    UStVA generieren
                  </button>
                  <button mat-stroked-button (click)="previewVATReport()">
                    <mat-icon>visibility</mat-icon>
                    Vorschau
                  </button>
                </div>
              </mat-card-content>
            </mat-card>
            
            <!-- VAT Summary -->
            <mat-card *ngIf="vatSummaryData" class="vat-summary-card">
              <mat-card-header>
                <mat-card-title>Umsatzsteuer-Zusammenfassung {{ vatReportYear }} Q{{ vatReportQuarter }}</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="vat-summary-grid">
                  <div class="vat-item">
                    <label>Umsätze 19% USt</label>
                    <span class="amount">{{ vatSummaryData.sales19 | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</span>
                  </div>
                  <div class="vat-item">
                    <label>daraus USt 19%</label>
                    <span class="amount">{{ vatSummaryData.vat19 | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</span>
                  </div>
                  <div class="vat-item">
                    <label>Umsätze 7% USt</label>
                    <span class="amount">{{ vatSummaryData.sales7 | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</span>
                  </div>
                  <div class="vat-item">
                    <label>daraus USt 7%</label>
                    <span class="amount">{{ vatSummaryData.vat7 | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</span>
                  </div>
                  <div class="vat-item total">
                    <label>Zahllast/Erstattung</label>
                    <span class="amount" [class.negative]="vatSummaryData.total < 0">
                      {{ vatSummaryData.total | currency:'EUR':'symbol':'1.2-2':'de-DE' }}
                    </span>
                  </div>
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>
        
        <!-- Category Reports Tab -->
        <mat-tab label="{{ 'reports.categoryReports' | translate }}">
          <div class="tab-content">
            <mat-card class="config-card">
              <mat-card-header>
                <mat-card-title>{{ 'reports.categoryReportsConfig' | translate }}</mat-card-title>
              </mat-card-header>
              <mat-card-content>
                <div class="form-row">
                  <mat-form-field>
                    <mat-label>Von Datum</mat-label>
                    <input matInput [matDatepicker]="fromPicker" [(ngModel)]="categoryFromDate">
                    <mat-datepicker-toggle matIconSuffix [for]="fromPicker"></mat-datepicker-toggle>
                    <mat-datepicker #fromPicker></mat-datepicker>
                  </mat-form-field>
                  
                  <mat-form-field>
                    <mat-label>Bis Datum</mat-label>
                    <input matInput [matDatepicker]="toPicker" [(ngModel)]="categoryToDate">
                    <mat-datepicker-toggle matIconSuffix [for]="toPicker"></mat-datepicker-toggle>
                    <mat-datepicker #toPicker></mat-datepicker>
                  </mat-form-field>
                  
                  <mat-form-field>
                    <mat-label>Kategorie</mat-label>
                    <mat-select [(value)]="selectedCategory" multiple>
                      <mat-option *ngFor="let category of availableCategories" [value]="category">
                        {{ category }}
                      </mat-option>
                    </mat-select>
                  </mat-form-field>
                  
                  <mat-form-field>
                    <mat-label>Format</mat-label>
                    <mat-select [(value)]="categoryReportFormat">
                      <mat-option value="PDF">PDF</mat-option>
                      <mat-option value="CSV">CSV</mat-option>
                      <mat-option value="EXCEL">Excel</mat-option>
                    </mat-select>
                  </mat-form-field>
                </div>
                
                <div class="action-buttons">
                  <button mat-raised-button color="primary" (click)="generateCategoryReport()" [disabled]="isGenerating">
                    <mat-icon>category</mat-icon>
                    Kategoriebericht generieren
                  </button>
                  <button mat-stroked-button (click)="previewCategoryReport()">
                    <mat-icon>visibility</mat-icon>
                    Vorschau
                  </button>
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>
      </mat-tab-group>
      
      <!-- Recent Reports -->
      <mat-card class="recent-reports-card">
        <mat-card-header>
          <mat-card-title>{{ 'reports.recentReports' | translate }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="recent-reports-list">
            <div *ngFor="let report of recentReports" class="recent-report-item">
              <div class="report-info">
                <span class="report-name">{{ report.name }}</span>
                <span class="report-date">{{ report.createdAt | date:'dd.MM.yyyy HH:mm':'de-DE' }}</span>
              </div>
              <div class="report-actions">
                <button mat-icon-button (click)="downloadReport(report)">
                  <mat-icon>file_download</mat-icon>
                </button>
                <button mat-icon-button (click)="deleteReport(report)">
                  <mat-icon>delete</mat-icon>
                </button>
              </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .reports-container {
      padding: 24px;
      background-color: #f5f5f5;
      min-height: 100vh;
    }
    
    .header-card {
      margin-bottom: 24px;
    }
    
    .reports-tabs {
      background-color: white;
      border-radius: 8px;
    }
    
    .tab-content {
      padding: 24px;
    }
    
    .config-card {
      margin-bottom: 24px;
    }
    
    .form-row {
      display: flex;
      gap: 16px;
      flex-wrap: wrap;
    }
    
    .form-row mat-form-field {
      min-width: 200px;
    }
    
    .action-buttons {
      display: flex;
      gap: 16px;
      margin-top: 16px;
      flex-wrap: wrap;
    }
    
    .action-buttons button {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .preview-card {
      margin-top: 24px;
    }
    
    .summary-chips {
      margin-left: auto;
    }
    
    .report-table {
      width: 100%;
      margin-top: 16px;
    }
    
    .summary-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 16px;
      margin-top: 24px;
    }
    
    .summary-card {
      text-align: center;
    }
    
    .summary-amount {
      font-size: 1.5rem;
      font-weight: bold;
      color: #1976d2;
      margin: 8px 0;
    }
    
    .summary-details {
      color: #666;
      font-size: 0.875rem;
    }
    
    .vat-summary-card {
      margin-top: 24px;
    }
    
    .vat-summary-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-top: 16px;
    }
    
    .vat-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px;
      background-color: #f9f9f9;
      border-radius: 4px;
    }
    
    .vat-item.total {
      background-color: #e3f2fd;
      font-weight: bold;
    }
    
    .vat-item .amount.negative {
      color: #f44336;
    }
    
    .recent-reports-card {
      margin-top: 24px;
    }
    
    .recent-report-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 0;
      border-bottom: 1px solid #e0e0e0;
    }
    
    .recent-report-item:last-child {
      border-bottom: none;
    }
    
    .report-info {
      display: flex;
      flex-direction: column;
    }
    
    .report-name {
      font-weight: 500;
    }
    
    .report-date {
      font-size: 0.875rem;
      color: #666;
    }
    
    .report-actions {
      display: flex;
      gap: 8px;
    }
    
    @media (max-width: 768px) {
      .form-row {
        flex-direction: column;
      }
      
      .form-row mat-form-field {
        min-width: unset;
        width: 100%;
      }
      
      .action-buttons {
        flex-direction: column;
      }
      
      .summary-grid {
        grid-template-columns: 1fr;
      }
      
      .vat-summary-grid {
        grid-template-columns: 1fr;
      }
    }
  `],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatChipsModule,
    TranslateModule
  ]
})
export class ReportsComponent implements OnInit {
  // Form data
  monthlyReportYear = new Date().getFullYear();
  monthlyReportMonth = new Date().getMonth() + 1;
  monthlyReportFormat: 'PDF' | 'CSV' | 'EXCEL' = 'PDF';
  
  yearlyReportYear = new Date().getFullYear();
  yearlyReportFormat: 'PDF' | 'CSV' | 'EXCEL' = 'PDF';
  
  vatReportYear = new Date().getFullYear();
  vatReportQuarter = Math.ceil((new Date().getMonth() + 1) / 3);
  vatReportFormat: 'PDF' | 'CSV' | 'EXCEL' = 'PDF';
  
  categoryFromDate = new Date(new Date().getFullYear(), 0, 1);
  categoryToDate = new Date();
  selectedCategory: string[] = [];
  categoryReportFormat: 'PDF' | 'CSV' | 'EXCEL' = 'PDF';
  
  // Data
  monthlyReportData: ReportData[] = [];
  yearlySummaryData: CategorySummary[] = [];
  vatSummaryData: any = null;
  
  // Configuration
  isGenerating = false;
  displayedColumns: string[] = ['date', 'category', 'description', 'amount'];
  
  availableYears: number[] = [];
  germanMonths = [
    'Januar', 'Februar', 'März', 'April', 'Mai', 'Juni',
    'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'
  ];
  
  availableCategories = [
    'Büroausstattung', 'Marketing', 'Reisekosten', 'Software',
    'Bewirtung', 'Fortbildung', 'Miete', 'Versicherungen', 'Sonstiges'
  ];
  
  recentReports = [
    { name: 'Monatsbericht Juni 2024.pdf', createdAt: new Date('2024-07-01'), format: 'PDF', type: 'monthly' },
    { name: 'UStVA Q2 2024.pdf', createdAt: new Date('2024-07-15'), format: 'PDF', type: 'vat' },
    { name: 'Kategoriebericht_Marketing.xlsx', createdAt: new Date('2024-07-20'), format: 'EXCEL', type: 'category' }
  ];
  
  constructor(
    private reportService: ReportService,
    private exportService: ExportService
  ) {}
  
  ngOnInit(): void {
    this.initializeAvailableYears();
    this.loadInitialData();
  }
  
  private initializeAvailableYears(): void {
    const currentYear = new Date().getFullYear();
    for (let year = currentYear; year >= currentYear - 5; year--) {
      this.availableYears.push(year);
    }
  }
  
  private loadInitialData(): void {
    // Load initial data for dropdowns and recent reports
  }
  
  // Monthly Reports
  generateMonthlyReport(): void {
    this.isGenerating = true;
    this.reportService.generateMonthlyReport(
      this.monthlyReportYear,
      this.monthlyReportMonth,
      this.monthlyReportFormat
    ).subscribe({
      next: (blob) => {
        const filename = `Monatsbericht_${this.monthlyReportYear}-${this.monthlyReportMonth.toString().padStart(2, '0')}.${this.monthlyReportFormat.toLowerCase()}`;
        this.downloadFile(blob, filename);
        this.isGenerating = false;
      },
      error: (error) => {
        console.error('Fehler beim Generieren des Monatsberichts:', error);
        this.isGenerating = false;
      }
    });
  }
  
  previewMonthlyReport(): void {
    // Mock data for preview
    this.monthlyReportData = [
      {
        id: '1',
        date: new Date(2024, this.monthlyReportMonth - 1, 5),
        category: 'Büroausstattung',
        description: 'Laptop für Buchhaltung',
        amount: 1200.00
      },
      {
        id: '2',
        date: new Date(2024, this.monthlyReportMonth - 1, 12),
        category: 'Marketing',
        description: 'Google Ads Kampagne',
        amount: 450.00
      },
      {
        id: '3',
        date: new Date(2024, this.monthlyReportMonth - 1, 20),
        category: 'Software',
        description: 'Adobe Creative Cloud',
        amount: 59.99
      }
    ];
  }
  
  // Yearly Reports
  generateYearlyReport(): void {
    this.isGenerating = true;
    this.reportService.generateYearlyReport(this.yearlyReportYear, this.yearlyReportFormat).subscribe({
      next: (blob) => {
        const filename = `Jahresbericht_${this.yearlyReportYear}.${this.yearlyReportFormat.toLowerCase()}`;
        this.downloadFile(blob, filename);
        this.isGenerating = false;
      },
      error: (error) => {
        console.error('Fehler beim Generieren des Jahresberichts:', error);
        this.isGenerating = false;
      }
    });
  }
  
  previewYearlyReport(): void {
    // Mock data for yearly summary
    this.yearlySummaryData = [
      { category: 'Büroausstattung', totalAmount: 15600, count: 12, percentage: 35.2 },
      { category: 'Marketing', totalAmount: 8900, count: 24, percentage: 20.1 },
      { category: 'Software', totalAmount: 7200, count: 36, percentage: 16.3 },
      { category: 'Reisekosten', totalAmount: 6400, count: 8, percentage: 14.5 },
      { category: 'Sonstiges', totalAmount: 6100, count: 18, percentage: 13.9 }
    ];
  }
  
  // VAT Reports
  generateVATReport(): void {
    this.isGenerating = true;
    this.reportService.generateVATReport(
      this.vatReportYear,
      this.vatReportQuarter,
      this.vatReportFormat
    ).subscribe({
      next: (blob) => {
        const filename = `UStVA_${this.vatReportYear}_Q${this.vatReportQuarter}.${this.vatReportFormat.toLowerCase()}`;
        this.downloadFile(blob, filename);
        this.isGenerating = false;
      },
      error: (error) => {
        console.error('Fehler beim Generieren des USt-Berichts:', error);
        this.isGenerating = false;
      }
    });
  }
  
  previewVATReport(): void {
    // Mock data for VAT summary
    this.vatSummaryData = {
      sales19: 25000.00,
      vat19: 4750.00,
      sales7: 5000.00,
      vat7: 350.00,
      total: 5100.00
    };
  }
  
  // Category Reports
  generateCategoryReport(): void {
    this.isGenerating = true;
    this.reportService.generateCategoryReport(
      this.categoryFromDate,
      this.categoryToDate,
      this.selectedCategory,
      this.categoryReportFormat
    ).subscribe({
      next: (blob) => {
        const categories = this.selectedCategory.length > 0 ? this.selectedCategory.join('_') : 'Alle';
        const filename = `Kategoriebericht_${categories}.${this.categoryReportFormat.toLowerCase()}`;
        this.downloadFile(blob, filename);
        this.isGenerating = false;
      },
      error: (error) => {
        console.error('Fehler beim Generieren des Kategorieberichts:', error);
        this.isGenerating = false;
      }
    });
  }
  
  previewCategoryReport(): void {
    console.log('Vorschau Kategoriebericht:', {
      from: this.categoryFromDate,
      to: this.categoryToDate,
      categories: this.selectedCategory
    });
  }
  
  // Helper methods
  getSelectedMonth(): string {
    return this.germanMonths[this.monthlyReportMonth - 1];
  }
  
  getTotalAmount(): number {
    return this.monthlyReportData.reduce((sum, item) => sum + item.amount, 0);
  }
  
  downloadReport(report: any): void {
    console.log('Download report:', report.name);
  }
  
  deleteReport(report: any): void {
    console.log('Delete report:', report.name);
  }
  
  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}