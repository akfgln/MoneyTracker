import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { TranslateModule } from '@ngx-translate/core';
import { ChartConfigService } from '../../core/services/chart-config.service';
import { ReportService } from '../../core/services/report.service';
import { TestDataService } from '../../services/test-data.service';

@Component({
  selector: 'app-dashboard',
  template: `
    <div class="dashboard-container">
      <!-- Summary Cards -->
      <mat-grid-list cols="4" rowHeight="200px" gutterSize="16px">
        <mat-grid-tile>
          <mat-card class="summary-card income-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>trending_up</mat-icon>
              <mat-card-title>{{ 'dashboard.totalIncome' | translate }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="amount">{{ totalIncome | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</div>
              <div class="change positive">+5.2% vs letzter Monat</div>
            </mat-card-content>
          </mat-card>
        </mat-grid-tile>
        
        <mat-grid-tile>
          <mat-card class="summary-card expense-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>trending_down</mat-icon>
              <mat-card-title>{{ 'dashboard.totalExpenses' | translate }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="amount">{{ totalExpenses | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</div>
              <div class="change negative">+2.1% vs letzter Monat</div>
            </mat-card-content>
          </mat-card>
        </mat-grid-tile>
        
        <mat-grid-tile>
          <mat-card class="summary-card profit-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>account_balance</mat-icon>
              <mat-card-title>{{ 'dashboard.netProfit' | translate }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="amount">{{ netProfit | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</div>
              <div class="change positive">+8.7% vs letzter Monat</div>
            </mat-card-content>
          </mat-card>
        </mat-grid-tile>
        
        <mat-grid-tile>
          <mat-card class="summary-card tax-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>receipt</mat-icon>
              <mat-card-title>{{ 'dashboard.estimatedTax' | translate }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="amount">{{ estimatedTax | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</div>
              <div class="tax-rate">19% MwSt.</div>
            </mat-card-content>
          </mat-card>
        </mat-grid-tile>
      </mat-grid-list>
      
      <!-- Charts Section -->
      <div class="charts-section">
        <div class="chart-row">
          <mat-card class="chart-card trend-chart">
            <mat-card-header>
              <mat-card-title>{{ 'dashboard.monthlyTrend' | translate }}</mat-card-title>
              <button mat-icon-button>
                <mat-icon>more_vert</mat-icon>
              </button>
            </mat-card-header>
            <mat-card-content>
              <div class="chart-container">
                <canvas baseChart
                        [data]="trendChartData"
                        [options]="trendChartOptions"
                        [type]="'line'">
                </canvas>
              </div>
            </mat-card-content>
          </mat-card>
          
          <mat-card class="chart-card category-chart">
            <mat-card-header>
              <mat-card-title>{{ 'dashboard.expensesByCategory' | translate }}</mat-card-title>
              <button mat-icon-button>
                <mat-icon>more_vert</mat-icon>
              </button>
            </mat-card-header>
            <mat-card-content>
              <div class="chart-container">
                <canvas baseChart
                        [data]="categoryChartData"
                        [options]="categoryChartOptions"
                        [type]="'doughnut'">
                </canvas>
              </div>
            </mat-card-content>
          </mat-card>
        </div>
      </div>
      
      <!-- Budget Tracking -->
      <mat-card class="budget-card">
        <mat-card-header>
          <mat-card-title>{{ 'dashboard.budgetTracking' | translate }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="budget-items">
            <div class="budget-item" *ngFor="let budget of budgetData">
              <div class="budget-header">
                <span class="category-name">{{ budget.category }}</span>
                <span class="budget-amount">{{ budget.spent | currency:'EUR':'symbol':'1.2-2':'de-DE' }} / {{ budget.limit | currency:'EUR':'symbol':'1.2-2':'de-DE' }}</span>
              </div>
              <div class="budget-progress">
                <div class="progress-bar">
                  <div class="progress-fill" [style.width.%]="budget.percentage" [class.over-budget]="budget.percentage > 100"></div>
                </div>
                <span class="percentage">{{ budget.percentage }}%</span>
              </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
      
      <!-- Quick Actions -->
      <mat-card class="actions-card">
        <mat-card-header>
          <mat-card-title>{{ 'dashboard.quickActions' | translate }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="action-buttons">
            <button mat-raised-button color="primary" (click)="generateReport('monthly')">
              <mat-icon>description</mat-icon>
              {{ 'dashboard.generateMonthlyReport' | translate }}
            </button>
            <button mat-raised-button color="accent" (click)="generateReport('tax')">
              <mat-icon>receipt_long</mat-icon>
              {{ 'dashboard.generateTaxReport' | translate }}
            </button>
            <button mat-raised-button (click)="exportData('csv')">
              <mat-icon>file_download</mat-icon>
              {{ 'dashboard.exportToCsv' | translate }}
            </button>
            <button mat-raised-button (click)="exportData('excel')">
              <mat-icon>table_chart</mat-icon>
              {{ 'dashboard.exportToExcel' | translate }}
            </button>
          </div>
        </mat-card-content>
      </mat-card>
      
      <!-- Test File Generation Section -->
      <mat-card class="test-actions-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>science</mat-icon>
            Test: Echte Datei-Generierung
          </mat-card-title>
          <mat-card-subtitle>
            Demonstration der authentischen PDF, Excel und CSV Generierung
          </mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div class="test-action-buttons">
            <button mat-stroked-button color="primary" (click)="testPDFGeneration()">
              <mat-icon>picture_as_pdf</mat-icon>
              Test PDF (jsPDF)
            </button>
            <button mat-stroked-button color="accent" (click)="testExcelGeneration()">
              <mat-icon>table_chart</mat-icon>
              Test Excel (XLSX)
            </button>
            <button mat-stroked-button (click)="testCSVGeneration()">
              <mat-icon>description</mat-icon>
              Test CSV (German)
            </button>
            <button mat-stroked-button color="warn" (click)="testAPIIntegration()">
              <mat-icon>api</mat-icon>
              Test API
            </button>
          </div>
          <p class="test-description">
            <mat-icon>info</mat-icon>
            Diese Buttons demonstrieren die echte Datei-Generierung mit deutschen Formaten.
          </p>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 24px;
      background-color: #f5f5f5;
      min-height: 100vh;
    }
    
    .summary-card {
      height: 100%;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
    }
    
    .summary-card .amount {
      font-size: 2rem;
      font-weight: bold;
      margin: 8px 0;
    }
    
    .summary-card .change {
      font-size: 0.875rem;
      font-weight: 500;
    }
    
    .change.positive { color: #4caf50; }
    .change.negative { color: #f44336; }
    
    .income-card { border-left: 4px solid #4caf50; }
    .expense-card { border-left: 4px solid #f44336; }
    .profit-card { border-left: 4px solid #2196f3; }
    .tax-card { border-left: 4px solid #ff9800; }
    
    .charts-section {
      margin: 24px 0;
    }
    
    .chart-row {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 16px;
    }
    
    .chart-card {
      padding: 16px;
    }
    
    .chart-container {
      height: 300px;
      position: relative;
    }
    
    .budget-card, .actions-card {
      margin: 16px 0;
    }
    
    .budget-item {
      margin: 16px 0;
    }
    
    .budget-header {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
    }
    
    .budget-progress {
      display: flex;
      align-items: center;
      gap: 12px;
    }
    
    .progress-bar {
      flex: 1;
      height: 8px;
      background-color: #e0e0e0;
      border-radius: 4px;
      overflow: hidden;
    }
    
    .progress-fill {
      height: 100%;
      background-color: #4caf50;
      transition: width 0.3s ease;
    }
    
    .progress-fill.over-budget {
      background-color: #f44336;
    }
    
    .action-buttons {
      display: flex;
      gap: 16px;
      flex-wrap: wrap;
    }
    
    .action-buttons button {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .test-actions-card {
      margin: 16px 0;
      border: 2px dashed #2196f3;
      background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%);
    }
    
    .test-action-buttons {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
      margin-bottom: 16px;
    }
    
    .test-action-buttons button {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 0.875rem;
    }
    
    .test-description {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 0.875rem;
      color: #1976d2;
      background: rgba(25, 118, 210, 0.1);
      padding: 12px;
      border-radius: 6px;
      margin: 0;
    }
    
    @media (max-width: 768px) {
      mat-grid-list {
        grid-template-columns: repeat(2, 1fr);
      }
      
      .chart-row {
        grid-template-columns: 1fr;
      }
      
      .action-buttons {
        flex-direction: column;
      }
    }
  `],
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatGridListModule,
    MatSnackBarModule,
    BaseChartDirective,
    TranslateModule
  ]
})
export class DashboardComponent implements OnInit {
  // Summary data
  totalIncome = 15750.50;
  totalExpenses = 8920.25;
  netProfit = 6830.25;
  estimatedTax = 1297.75;
  
  // Chart data
  trendChartData: ChartConfiguration['data'] = {
    labels: ['Jan', 'Feb', 'Mär', 'Apr', 'Mai', 'Jun'],
    datasets: [
      {
        label: 'Einnahmen',
        data: [12000, 15000, 13500, 16000, 14500, 15750],
        borderColor: '#4caf50',
        backgroundColor: 'rgba(76, 175, 80, 0.1)',
        tension: 0.4
      },
      {
        label: 'Ausgaben',
        data: [8000, 9500, 8800, 10200, 9100, 8920],
        borderColor: '#f44336',
        backgroundColor: 'rgba(244, 67, 54, 0.1)',
        tension: 0.4
      }
    ]
  };
  
  categoryChartData: ChartConfiguration['data'] = {
    labels: ['Büroausstattung', 'Marketing', 'Reisekosten', 'Software', 'Sonstiges'],
    datasets: [{
      data: [2500, 1800, 1200, 2400, 1020],
      backgroundColor: [
        '#FF6384',
        '#36A2EB',
        '#FFCE56',
        '#4BC0C0',
        '#9966FF'
      ]
    }]
  };
  
  trendChartOptions: ChartConfiguration['options'] = {};
  categoryChartOptions: ChartConfiguration['options'] = {};
  
  // Budget data
  budgetData = [
    { category: 'Marketing', spent: 1800, limit: 2000, percentage: 90 },
    { category: 'Büroausstattung', spent: 2500, limit: 2200, percentage: 114 },
    { category: 'Software', spent: 2400, limit: 3000, percentage: 80 },
    { category: 'Reisekosten', spent: 1200, limit: 1500, percentage: 80 }
  ];
  
  constructor(
    private chartConfigService: ChartConfigService,
    private reportService: ReportService,
    private testDataService: TestDataService,
    private snackBar: MatSnackBar
  ) {}
  
  ngOnInit(): void {
    this.setupChartOptions();
    this.loadDashboardData();
  }
  
  private setupChartOptions(): void {
    this.trendChartOptions = this.chartConfigService.getGermanTrendChartOptions();
    this.categoryChartOptions = this.chartConfigService.getGermanCategoryChartOptions();
  }
  
  private loadDashboardData(): void {
    // Load real data from API
    // This will be implemented with actual API calls
  }
  
  generateReport(type: 'monthly' | 'tax'): void {
    const currentDate = new Date();
    const year = currentDate.getFullYear();
    const month = currentDate.getMonth() + 1;
    
    if (type === 'monthly') {
      this.reportService.generateMonthlyReport(year, month, 'PDF').subscribe({
        next: (blob) => this.downloadFile(blob, `Monatsbericht_${year}-${month.toString().padStart(2, '0')}.pdf`),
        error: (error) => console.error('Fehler beim Generieren des Monatsberichts:', error)
      });
    } else if (type === 'tax') {
      this.reportService.generateTaxReport(year, 'PDF').subscribe({
        next: (blob) => this.downloadFile(blob, `Steuerbericht_${year}.pdf`),
        error: (error) => console.error('Fehler beim Generieren des Steuerberichts:', error)
      });
    }
  }
  
  exportData(format: 'csv' | 'excel'): void {
    this.showMessage(`Exportiere Daten als ${format.toUpperCase()}...`);
    console.log(`Exportiere Daten als ${format.toUpperCase()}`);
  }
  
  /**
   * Test real PDF generation using jsPDF
   */
  async testPDFGeneration(): Promise<void> {
    this.showMessage('Generiere echte PDF-Datei...', 'info');
    try {
      await this.testDataService.testPDFGeneration();
      this.showMessage('PDF erfolgreich generiert und heruntergeladen!', 'success');
    } catch (error) {
      console.error('PDF generation error:', error);
      this.showMessage('Fehler beim Generieren der PDF', 'error');
    }
  }
  
  /**
   * Test real Excel generation using XLSX library
   */
  async testExcelGeneration(): Promise<void> {
    this.showMessage('Generiere echte Excel-Datei...', 'info');
    try {
      await this.testDataService.testExcelGeneration();
      this.showMessage('Excel-Datei erfolgreich generiert und heruntergeladen!', 'success');
    } catch (error) {
      console.error('Excel generation error:', error);
      this.showMessage('Fehler beim Generieren der Excel-Datei', 'error');
    }
  }
  
  /**
   * Test CSV generation with German formatting
   */
  async testCSVGeneration(): Promise<void> {
    this.showMessage('Generiere CSV mit deutscher Formatierung...', 'info');
    try {
      await this.testDataService.testCSVGeneration();
      this.showMessage('CSV-Datei erfolgreich generiert und heruntergeladen!', 'success');
    } catch (error) {
      console.error('CSV generation error:', error);
      this.showMessage('Fehler beim Generieren der CSV-Datei', 'error');
    }
  }
  
  /**
   * Test API integration
   */
  testAPIIntegration(): void {
    this.showMessage('Teste API-Integration...', 'info');
    this.testDataService.demonstrateAPIIntegration().subscribe({
      next: (result) => {
        console.log('API Health Check Result:', result);
        const statusMessage = result.status === 'offline' 
          ? 'API offline - Mock-Daten werden verwendet'
          : `API online - Status: ${result.status}`;
        this.showMessage(statusMessage, result.status === 'offline' ? 'warn' : 'success');
      },
      error: (error) => {
        console.error('API test error:', error);
        this.showMessage('API-Test fehlgeschlagen - Mock-Daten verfügbar', 'warn');
      }
    });
  }
  
  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }
  
  /**
   * Show user feedback message
   */
  private showMessage(message: string, type: 'success' | 'error' | 'info' | 'warn' = 'info'): void {
    const panelClass = {
      'success': ['success-snackbar'],
      'error': ['error-snackbar'],
      'info': ['info-snackbar'],
      'warn': ['warn-snackbar']
    }[type];
    
    this.snackBar.open(message, 'OK', {
      duration: type === 'error' ? 5000 : 3000,
      panelClass,
      horizontalPosition: 'center',
      verticalPosition: 'bottom'
    });
  }
}