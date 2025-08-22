import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import { ExportOptions, CSVExportData, PDFExportData } from '../models/dashboard.types';
// Import jsPDF for PDF generation
declare var jsPDF: any;
// Import xlsx for Excel generation
import * as XLSX from 'xlsx';

@Injectable({
  providedIn: 'root'
})
export class ExportService {
  private readonly locale = 'de-DE';
  private readonly currency = 'EUR';

  constructor() {}

  /**
   * Export data in specified format
   */
  async exportData(
    data: any[],
    options: ExportOptions
  ): Promise<void> {
    try {
      switch (options.format) {
        case 'CSV':
          this.exportToCSV(data, options);
          break;
        case 'EXCEL':
          this.exportToExcel(data, options);
          break;
        case 'PDF':
          await this.exportToPDF(data, options);
          break;
        default:
          throw new Error(`Unsupported format: ${options.format}`);
      }
    } catch (error) {
      console.error('Export failed:', error);
      throw error;
    }
  }

  /**
   * Export to German-formatted CSV
   */
  private exportToCSV(data: any[], options: ExportOptions): void {
    if (!data || data.length === 0) {
      throw new Error('Keine Daten zum Exportieren vorhanden');
    }

    const csvData = this.convertToGermanCSV(data);
    const blob = new Blob([csvData], { 
      type: 'text/csv;charset=utf-8;' 
    });
    
    this.downloadFile(blob, options.filename, 'csv');
  }

  /**
   * Export to Excel with German formatting
   */
  private exportToExcel(data: any[], options: ExportOptions): void {
    if (!data || data.length === 0) {
      throw new Error('Keine Daten zum Exportieren vorhanden');
    }

    // Convert data to German format
    const formattedData = this.formatDataForExcel(data);
    
    // Create workbook
    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.json_to_sheet(formattedData, {
      header: this.getGermanHeaders(data[0]),
      dateNF: 'DD.MM.YYYY' // German date format
    });

    // Set column widths
    const colWidths = this.calculateColumnWidths(formattedData);
    ws['!cols'] = colWidths;

    // Add German formatting
    this.applyGermanExcelFormatting(ws, formattedData);
    
    // Add worksheet to workbook
    XLSX.utils.book_append_sheet(wb, ws, 'Daten');
    
    // Write and download
    const buffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([buffer], { 
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
    });
    
    this.downloadFile(blob, options.filename, 'xlsx');
  }

  /**
   * Export to PDF with German formatting
   */
  private async exportToPDF(data: any[], options: ExportOptions): Promise<void> {
    // For production, you would typically load jsPDF dynamically
    // const { jsPDF } = await import('jspdf');
    
    // For now, we'll create a simple implementation
    const pdfData = this.preparePDFData(data, options);
    const htmlContent = this.generatePDFHTML(pdfData);
    
    // Create PDF using browser's print functionality as fallback
    this.printToPDF(htmlContent, options.filename);
  }

  /**
   * Convert data to German CSV format
   */
  private convertToGermanCSV(data: any[]): string {
    if (!data || data.length === 0) return '';
    
    // Get headers and translate them
    const headers = Object.keys(data[0]);
    const germanHeaders = headers.map(header => this.translateHeader(header));
    
    // Convert data rows
    const rows = data.map(item => 
      headers.map(header => this.formatCSVValue(item[header]))
    );
    
    // Combine headers and rows
    const csvContent = [germanHeaders, ...rows]
      .map(row => row.map(field => `"${field}"`).join(';')) // German CSV uses semicolon
      .join('\n');
    
    // Add BOM for proper German character encoding
    return '\ufeff' + csvContent;
  }

  /**
   * Format data for Excel export
   */
  private formatDataForExcel(data: any[]): any[] {
    return data.map(item => {
      const formatted: any = {};
      
      Object.keys(item).forEach(key => {
        const germanKey = this.translateHeader(key);
        formatted[germanKey] = this.formatExcelValue(item[key]);
      });
      
      return formatted;
    });
  }

  /**
   * Get German headers for Excel
   */
  private getGermanHeaders(sampleItem: any): string[] {
    return Object.keys(sampleItem).map(key => this.translateHeader(key));
  }

  /**
   * Apply German formatting to Excel worksheet
   */
  private applyGermanExcelFormatting(ws: any, data: any[]): void {
    // Set number format for currency cells
    Object.keys(ws).forEach(cellAddress => {
      if (cellAddress.startsWith('!')) return;
      
      const cell = ws[cellAddress];
      if (typeof cell.v === 'number') {
        // Check if this looks like currency (has decimal places)
        if (cell.v % 1 !== 0) {
          cell.z = '#.##0,00 "€"'; // German currency format
        } else {
          cell.z = '#.##0'; // German number format
        }
      }
    });
  }

  /**
   * Calculate column widths for Excel
   */
  private calculateColumnWidths(data: any[]): any[] {
    if (!data || data.length === 0) return [];
    
    const headers = Object.keys(data[0]);
    return headers.map(header => {
      const maxLength = Math.max(
        header.length,
        ...data.map(row => {
          const value = row[header];
          return value ? value.toString().length : 0;
        })
      );
      
      return { width: Math.min(maxLength + 2, 50) };
    });
  }

  /**
   * Prepare data for PDF export
   */
  private preparePDFData(data: any[], options: ExportOptions): PDFExportData {
    return {
      title: options.filename,
      subtitle: `Erstellt am ${this.formatGermanDate(new Date())}`,
      sections: [
        {
          title: 'Datenübersicht',
          type: 'table',
          content: {
            headers: this.getGermanHeaders(data[0]),
            rows: data.map(item => 
              Object.keys(item).map(key => this.formatPDFValue(item[key]))
            )
          }
        }
      ],
      footer: `Generiert von Money Tracker am ${this.formatGermanDateTime(new Date())}`
    };
  }

  /**
   * Generate HTML for PDF
   */
  private generatePDFHTML(pdfData: PDFExportData): string {
    let html = `
      <!DOCTYPE html>
      <html lang="de">
      <head>
        <meta charset="UTF-8">
        <title>${pdfData.title}</title>
        <style>
          body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 20px;
            color: #333;
          }
          .header {
            text-align: center;
            margin-bottom: 30px;
            border-bottom: 2px solid #1976D2;
            padding-bottom: 15px;
          }
          .title {
            font-size: 24px;
            font-weight: bold;
            color: #1976D2;
            margin-bottom: 5px;
          }
          .subtitle {
            font-size: 14px;
            color: #666;
          }
          .section {
            margin-bottom: 30px;
          }
          .section-title {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 15px;
            color: #1976D2;
            border-left: 4px solid #1976D2;
            padding-left: 10px;
          }
          table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
            font-size: 12px;
          }
          th, td {
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
          }
          th {
            background-color: #f5f5f5;
            font-weight: bold;
            color: #333;
          }
          tr:nth-child(even) {
            background-color: #f9f9f9;
          }
          .footer {
            margin-top: 40px;
            padding-top: 15px;
            border-top: 1px solid #ddd;
            font-size: 10px;
            color: #666;
            text-align: center;
          }
          .currency {
            text-align: right;
            font-weight: bold;
          }
          @media print {
            body { margin: 0; }
            .header { page-break-after: avoid; }
          }
        </style>
      </head>
      <body>
        <div class="header">
          <div class="title">${pdfData.title}</div>
          <div class="subtitle">${pdfData.subtitle || ''}</div>
        </div>
    `;

    // Add sections
    pdfData.sections.forEach(section => {
      html += `<div class="section">`;
      html += `<div class="section-title">${section.title}</div>`;
      
      if (section.type === 'table' && section.content) {
        const tableData = section.content as any;
        html += `<table>`;
        
        // Headers
        html += `<thead><tr>`;
        tableData.headers.forEach((header: string) => {
          html += `<th>${header}</th>`;
        });
        html += `</tr></thead>`;
        
        // Rows
        html += `<tbody>`;
        tableData.rows.forEach((row: any[]) => {
          html += `<tr>`;
          row.forEach((cell, index) => {
            const className = this.isCurrency(tableData.headers[index]) ? 'currency' : '';
            html += `<td class="${className}">${cell}</td>`;
          });
          html += `</tr>`;
        });
        html += `</tbody></table>`;
      }
      
      html += `</div>`;
    });

    // Add footer
    if (pdfData.footer) {
      html += `<div class="footer">${pdfData.footer}</div>`;
    }

    html += `</body></html>`;
    return html;
  }

  /**
   * Print HTML to PDF using browser functionality
   */
  private printToPDF(htmlContent: string, filename: string): void {
    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(htmlContent);
      printWindow.document.close();
      
      printWindow.onload = () => {
        printWindow.print();
        // Note: In a real implementation, you might want to use a library like jsPDF
        // that can generate actual PDF files instead of relying on browser print
      };
    }
  }

  /**
   * Format value for CSV export
   */
  private formatCSVValue(value: any): string {
    if (value === null || value === undefined) return '';
    
    if (typeof value === 'number') {
      // German number formatting
      return value.toLocaleString(this.locale, {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      });
    }
    
    if (value instanceof Date) {
      // German date formatting
      return value.toLocaleDateString(this.locale);
    }
    
    return value.toString().replace(/"/g, '""'); // Escape quotes
  }

  /**
   * Format value for Excel export
   */
  private formatExcelValue(value: any): any {
    if (value === null || value === undefined) return '';
    
    if (typeof value === 'number') {
      return value; // Excel will handle formatting
    }
    
    if (value instanceof Date) {
      return value; // Excel will handle date formatting
    }
    
    return value;
  }

  /**
   * Format value for PDF export
   */
  private formatPDFValue(value: any): string {
    if (value === null || value === undefined) return '';
    
    if (typeof value === 'number') {
      // Check if it looks like currency
      if (Math.abs(value) >= 0.01 && value % 1 !== 0) {
        return value.toLocaleString(this.locale, {
          style: 'currency',
          currency: this.currency
        });
      } else {
        return value.toLocaleString(this.locale);
      }
    }
    
    if (value instanceof Date) {
      return value.toLocaleDateString(this.locale);
    }
    
    return value.toString();
  }

  /**
   * Translate header to German
   */
  private translateHeader(key: string): string {
    const translations: { [key: string]: string } = {
      'id': 'ID',
      'date': 'Datum',
      'transactionDate': 'Transaktionsdatum',
      'description': 'Beschreibung',
      'amount': 'Betrag',
      'category': 'Kategorie',
      'categoryName': 'Kategorie',
      'merchantName': 'Händler',
      'merchant': 'Händler',
      'vatAmount': 'MwSt.-Betrag',
      'vatRate': 'MwSt.-Satz',
      'netAmount': 'Nettobetrag',
      'grossAmount': 'Bruttobetrag',
      'transactionType': 'Transaktionsart',
      'accountName': 'Konto',
      'balance': 'Saldo',
      'income': 'Einnahmen',
      'expenses': 'Ausgaben',
      'month': 'Monat',
      'year': 'Jahr',
      'quarter': 'Quartal',
      'total': 'Gesamt',
      'count': 'Anzahl',
      'percentage': 'Prozent',
      'createdAt': 'Erstellt am',
      'updatedAt': 'Aktualisiert am'
    };
    
    return translations[key] || key;
  }

  /**
   * Check if header represents currency
   */
  private isCurrency(header: string): boolean {
    const currencyHeaders = [
      'betrag', 'amount', 'saldo', 'balance', 'einnahmen', 'income',
      'ausgaben', 'expenses', 'mwst', 'vat', 'netto', 'net', 'brutto', 'gross'
    ];
    
    return currencyHeaders.some(currencyHeader => 
      header.toLowerCase().includes(currencyHeader)
    );
  }

  /**
   * Download file
   */
  private downloadFile(blob: Blob, filename: string, extension: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `${filename}.${extension}`;
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    // Clean up
    window.URL.revokeObjectURL(url);
  }

  /**
   * Format German date
   */
  private formatGermanDate(date: Date): string {
    return date.toLocaleDateString(this.locale);
  }

  /**
   * Format German date and time
   */
  private formatGermanDateTime(date: Date): string {
    return date.toLocaleString(this.locale);
  }

  /**
   * Generate sample data for testing
   */
  generateSampleData(): any[] {
    return [
      {
        date: new Date('2024-01-15'),
        description: 'Supermarkt Einkauf',
        amount: -45.67,
        category: 'Lebensmittel',
        merchantName: 'REWE',
        vatAmount: -7.30,
        vatRate: 0.19
      },
      {
        date: new Date('2024-01-14'),
        description: 'Gehalt',
        amount: 3500.00,
        category: 'Einkommen',
        merchantName: 'Arbeitgeber GmbH',
        vatAmount: 0,
        vatRate: 0.00
      },
      {
        date: new Date('2024-01-13'),
        description: 'Tankstelle',
        amount: -65.80,
        category: 'Transport',
        merchantName: 'Shell',
        vatAmount: -10.52,
        vatRate: 0.19
      }
    ];
  }
}