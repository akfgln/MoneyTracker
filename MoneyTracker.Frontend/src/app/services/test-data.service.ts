import { Injectable } from '@angular/core';
import { ExportService } from '../core/services/export.service';
import { ReportService } from '../core/services/report.service';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TestDataService {
  
  constructor(
    private exportService: ExportService,
    private reportService: ReportService
  ) {}
  
  /**
   * Generate test financial data for demonstration
   */
  generateTestFinancialData() {
    return {
      transactions: this.generateTestTransactions(),
      monthlyReport: this.generateTestMonthlyReport(),
      yearlyReport: this.generateTestYearlyReport(),
      vatReport: this.generateTestVATReport()
    };
  }
  
  /**
   * Test real PDF generation
   */
  async testPDFGeneration(): Promise<void> {
    const testData = {
      title: 'Test PDF - Deutsches Finanz-Dashboard',
      subtitle: 'Beispiel einer echten PDF-Generierung',
      headers: ['Datum', 'Kategorie', 'Beschreibung', 'Betrag'],
      rows: [
        [new Date('2024-01-15'), 'Büroausstattung', 'Laptop Dell XPS', 1299.00],
        [new Date('2024-01-20'), 'Marketing', 'Facebook Werbung', 250.50],
        [new Date('2024-01-25'), 'Software', 'Microsoft Office Lizenz', 149.99],
        [new Date('2024-02-01'), 'Reisekosten', 'Kundenbesuch München', 89.40],
        [new Date('2024-02-10'), 'Bewirtung', 'Geschäftsessen', 67.80]
      ],
      metadata: {
        'Berichtszeitraum': 'Januar - Februar 2024',
        'Anzahl Transaktionen': 5,
        'Gesamtbetrag': 1856.69,
        'Erstellt am': new Date()
      },
      summary: {
        'Gesamteinnahmen': 0.00,
        'Gesamtausgaben': 1856.69,
        'Nettoergebnis': -1856.69,
        'Schätzung MwSt.': 296.93
      }
    };
    
    try {
      await this.exportService.downloadReport(
        testData,
        'Test_PDF_Bericht.pdf',
        'PDF',
        { germanLocale: true, includeMetadata: true }
      );
      console.log('PDF erfolgreich generiert und heruntergeladen!');
    } catch (error) {
      console.error('Fehler beim Generieren der PDF:', error);
    }
  }
  
  /**
   * Test real Excel generation
   */
  async testExcelGeneration(): Promise<void> {
    const testData = {
      title: 'Test Excel - Transaktionsdetails',
      headers: ['Datum', 'Kategorie', 'Beschreibung', 'Nettobetrag', 'MwSt-Satz', 'MwSt-Betrag', 'Bruttobetrag'],
      rows: [
        [new Date('2024-01-15'), 'Büroausstattung', 'Laptop Dell XPS', 1092.44, '19%', 207.56, 1300.00],
        [new Date('2024-01-20'), 'Marketing', 'Facebook Werbung', 210.50, '19%', 39.99, 250.49],
        [new Date('2024-01-25'), 'Software', 'Microsoft Office', 126.05, '19%', 23.95, 150.00],
        [new Date('2024-02-01'), 'Reisekosten', 'Bahnfahrt München', 75.13, '19%', 14.27, 89.40],
        [new Date('2024-02-10'), 'Bewirtung', 'Geschäftsessen', 56.98, '19%', 10.83, 67.81]
      ],
      metadata: {
        'Berichtszeitraum': 'Januar - Februar 2024',
        'Unternehmen': 'Beispiel GmbH',
        'Steuernummer': 'DE123456789',
        'Erstellt von': 'Deutsches Finanz-Dashboard',
        'Erstellt am': new Date()
      },
      summary: {
        'Summe Netto': 1561.10,
        'Summe MwSt': 296.60,
        'Summe Brutto': 1857.70
      }
    };
    
    try {
      await this.exportService.downloadReport(
        testData,
        'Test_Excel_Transaktionen.xlsx',
        'EXCEL',
        { germanLocale: true, includeMetadata: true }
      );
      console.log('Excel-Datei erfolgreich generiert und heruntergeladen!');
    } catch (error) {
      console.error('Fehler beim Generieren der Excel-Datei:', error);
    }
  }
  
  /**
   * Test CSV generation with German formatting
   */
  async testCSVGeneration(): Promise<void> {
    const testData = {
      headers: ['Datum', 'Kategorie', 'Beschreibung', 'Betrag', 'Währung'],
      rows: [
        [new Date('2024-01-15'), 'Büroausstattung', 'Laptop mit Zubehör', 1299.00, 'EUR'],
        [new Date('2024-01-20'), 'Marketing', 'Online-Werbung "Kampagne Q1"', 250.50, 'EUR'],
        [new Date('2024-01-25'), 'Software', 'Jahreslizenz, erneuert', 149.99, 'EUR']
      ],
      metadata: {
        'Export-Typ': 'CSV mit deutscher Formatierung',
        'Zeichenkodierung': 'UTF-8 mit BOM',
        'Dezimaltrennzeichen': 'Komma',
        'Datums-Format': 'dd.MM.yyyy'
      }
    };
    
    try {
      await this.exportService.downloadReport(
        testData,
        'Test_CSV_Deutsche_Formatierung.csv',
        'CSV',
        { germanLocale: true, dateFormat: 'dd.MM.yyyy' }
      );
      console.log('CSV-Datei mit deutscher Formatierung erfolgreich generiert!');
    } catch (error) {
      console.error('Fehler beim Generieren der CSV-Datei:', error);
    }
  }
  
  /**
   * Demonstrate API integration patterns
   */
  demonstrateAPIIntegration(): Observable<any> {
    console.log('API Integration Test gestartet...');
    
    // Test API health check
    return this.reportService.checkApiHealth();
  }
  
  // Private helper methods for generating test data
  private generateTestTransactions() {
    return [
      {
        id: 'tx_001',
        date: new Date('2024-01-15'),
        category: 'Büroausstattung',
        description: 'Laptop Dell XPS 13',
        amount: 1299.00,
        taxRate: 19,
        vatAmount: 207.56,
        invoiceNumber: 'RE-2024-001'
      },
      {
        id: 'tx_002',
        date: new Date('2024-01-20'),
        category: 'Marketing',
        description: 'Facebook Werbekampagne',
        amount: 250.50,
        taxRate: 19,
        vatAmount: 39.99,
        invoiceNumber: 'RE-2024-002'
      }
      // ... more test transactions
    ];
  }
  
  private generateTestMonthlyReport() {
    return {
      year: 2024,
      month: 1,
      totalIncome: 15750.50,
      totalExpenses: 8920.25,
      netProfit: 6830.25,
      taxEstimate: 1297.75,
      transactionCount: 156,
      topCategories: [
        { category: 'Büroausstattung', amount: 2500.00 },
        { category: 'Marketing', amount: 1800.00 },
        { category: 'Software', amount: 1200.00 }
      ]
    };
  }
  
  private generateTestYearlyReport() {
    return {
      year: 2024,
      totalIncome: 189006.00,
      totalExpenses: 107043.00,
      netProfit: 81963.00,
      taxEstimate: 15573.97,
      transactionCount: 1247,
      monthlyBreakdown: [
        { month: 1, income: 15750.50, expenses: 8920.25 },
        { month: 2, income: 16200.00, expenses: 9100.50 }
        // ... more months
      ]
    };
  }
  
  private generateTestVATReport() {
    return {
      year: 2024,
      quarter: 1,
      sales19: 25000.00,
      vat19: 4750.00,
      sales7: 5000.00,
      vat7: 350.00,
      inputVat: 1200.00,
      totalLiability: 3900.00
    };
  }
}