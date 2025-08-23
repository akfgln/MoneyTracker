import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import jsPDF from 'jspdf';
import 'jspdf-autotable';
import * as XLSX from 'xlsx';

// Extend jsPDF interface for autoTable
declare module 'jspdf' {
  interface jsPDF {
    autoTable: (options: any) => jsPDF;
  }
}

export interface ExportData {
  headers: string[];
  rows: any[][];
  title?: string;
  subtitle?: string;
  metadata?: { [key: string]: any };
  summary?: { [key: string]: any };
}

export interface ExportOptions {
  filename: string;
  format: 'CSV' | 'EXCEL' | 'PDF';
  germanLocale?: boolean;
  includeHeader?: boolean;
  includeMetadata?: boolean;
  dateFormat?: 'dd.MM.yyyy' | 'dd/MM/yyyy' | 'yyyy-MM-dd';
  currencySymbol?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ExportService {
  
  constructor() {}
  
  /**
   * Download report data in the specified format
   */
  async downloadReport(
    data: any,
    filename: string,
    format: 'PDF' | 'CSV' | 'EXCEL',
    options?: {
      germanLocale?: boolean;
      includeMetadata?: boolean;
      dateFormat?: string;
    }
  ): Promise<void> {
    try {
      let blob: Blob;
      const exportOptions: ExportOptions = {
        filename,
        format,
        germanLocale: options?.germanLocale ?? true,
        includeHeader: true,
        includeMetadata: options?.includeMetadata ?? true,
        dateFormat: 'dd.MM.yyyy',
        currencySymbol: '€'
      };
      
      switch (format) {
        case 'CSV':
          blob = await this.generateCSV(data, exportOptions);
          break;
        case 'EXCEL':
          blob = await this.generateExcel(data, exportOptions);
          break;
        case 'PDF':
          blob = await this.generatePDF(data, exportOptions);
          break;
        default:
          throw new Error(`Nicht unterstütztes Format: ${format}`);
      }
      
      this.downloadFile(blob, filename);
    } catch (error) {
      console.error('Fehler beim Exportieren der Daten:', error);
      throw error;
    }
  }
  
  /**
   * Generate CSV blob with German formatting
   */
  private async generateCSV(data: any, options: ExportOptions): Promise<Blob> {
    let csvContent = '';
    
    // Add metadata header if requested
    if (options.includeMetadata && data.metadata) {
      csvContent += this.formatMetadataForCSV(data.metadata);
      csvContent += '\n\n';
    }
    
    // Add headers
    if (options.includeHeader && data.headers) {
      csvContent += this.escapeCSVRow(data.headers) + '\n';
    }
    
    // Add data rows
    if (data.rows && Array.isArray(data.rows)) {
      for (const row of data.rows) {
        const formattedRow = row.map((cell: any) => this.formatCellForGerman(cell, options));
        csvContent += this.escapeCSVRow(formattedRow) + '\n';
      }
    }
    
    // Add summary footer if available
    if (data.summary) {
      csvContent += '\n';
      csvContent += this.formatSummaryForCSV(data.summary, options);
    }
    
    // Add generation timestamp
    const timestamp = new Date().toLocaleString('de-DE');
    csvContent += `\n\n"Erstellt am:","${timestamp}"`;
    
    return new Blob([csvContent], {
      type: 'text/csv;charset=utf-8-sig' // BOM for German Excel compatibility
    });
  }
  
  /**
   * Generate authentic Excel blob using xlsx library
   */
  private async generateExcel(data: any, options: ExportOptions): Promise<Blob> {
    // Create a new workbook
    const workbook = XLSX.utils.book_new();
    
    // Main data worksheet
    const worksheetData: any[][] = [];
    
    // Add metadata if requested
    if (options.includeMetadata && data.metadata) {
      worksheetData.push(['Berichtsinformationen']);
      worksheetData.push([]); // Empty row
      
      for (const [key, value] of Object.entries(data.metadata)) {
        const formattedValue = this.formatCellForGerman(value, options);
        worksheetData.push([key, formattedValue]);
      }
      
      worksheetData.push([]); // Empty row
      worksheetData.push([]); // Empty row
    }
    
    // Add headers and data
    if (data.headers && data.rows) {
      worksheetData.push(data.headers);
      
      for (const row of data.rows) {
        const formattedRow = row.map((cell: any) => {
          if (typeof cell === 'number' && (cell % 1 !== 0 || cell > 999)) {
            // Keep as number for Excel formatting
            return cell;
          }
          return this.formatCellForGerman(cell, options);
        });
        worksheetData.push(formattedRow);
      }
    }
    
    // Add summary if available
    if (data.summary) {
      worksheetData.push([]);
      worksheetData.push(['Zusammenfassung']);
      for (const [key, value] of Object.entries(data.summary)) {
        worksheetData.push([key, typeof value === 'number' ? value : this.formatCellForGerman(value, options)]);
      }
    }
    
    // Create worksheet
    const worksheet = XLSX.utils.aoa_to_sheet(worksheetData);
    
    // Style the worksheet
    const range = XLSX.utils.decode_range(worksheet['!ref'] || 'A1');
    
    // Format currency columns (assuming amount columns)
    for (let row = range.s.r; row <= range.e.r; row++) {
      for (let col = range.s.c; col <= range.e.c; col++) {
        const cellAddress = XLSX.utils.encode_cell({ r: row, c: col });
        const cell = worksheet[cellAddress];
        
        if (cell && typeof cell.v === 'number' && (cell.v % 1 !== 0 || cell.v > 999)) {
          // Apply German currency formatting
          cell.z = '#,##0.00" €"';
        }
      }
    }
    
    // Set column widths
    worksheet['!cols'] = [
      { width: 20 }, // First column
      { width: 15 }, // Amount columns
      { width: 25 }, // Description columns
      { width: 15 }, // Additional columns
      { width: 15 }
    ];
    
    // Add worksheet to workbook
    XLSX.utils.book_append_sheet(workbook, worksheet, data.title || 'Bericht');
    
    // Generate Excel file
    const excelBuffer = XLSX.write(workbook, {
      bookType: 'xlsx',
      type: 'array',
      compression: true
    });
    
    return new Blob([excelBuffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });
  }
  
  /**
   * Generate authentic PDF using jsPDF library
   */
  private async generatePDF(data: any, options: ExportOptions): Promise<Blob> {
    const pdf = new jsPDF({
      orientation: 'portrait',
      unit: 'mm',
      format: 'a4'
    });
    
    // Set up German fonts and formatting
    pdf.setFont('helvetica');
    
    let yPosition = 20;
    const pageWidth = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();
    const margin = 20;
    
    // Helper function to check if we need a new page
    const checkNewPage = (requiredSpace: number = 10) => {
      if (yPosition + requiredSpace > pageHeight - margin) {
        pdf.addPage();
        yPosition = 20;
      }
    };
    
    // Add title
    if (data.title) {
      pdf.setFontSize(20);
      pdf.setTextColor(51, 51, 51); // Dark gray
      pdf.text(data.title, margin, yPosition);
      yPosition += 10;
      
      // Add underline
      pdf.setLineWidth(0.5);
      pdf.line(margin, yPosition, pageWidth - margin, yPosition);
      yPosition += 10;
    }
    
    // Add subtitle
    if (data.subtitle) {
      checkNewPage(15);
      pdf.setFontSize(12);
      pdf.setTextColor(102, 102, 102);
      pdf.text(data.subtitle, margin, yPosition);
      yPosition += 10;
    }
    
    // Add metadata
    if (options.includeMetadata && data.metadata) {
      checkNewPage(30);
      pdf.setFontSize(14);
      pdf.setTextColor(51, 51, 51);
      pdf.text('Berichtsinformationen', margin, yPosition);
      yPosition += 8;
      
      pdf.setFontSize(10);
      pdf.setTextColor(85, 85, 85);
      
      for (const [key, value] of Object.entries(data.metadata)) {
        checkNewPage(5);
        const formattedValue = this.formatCellForGerman(value, options);
        pdf.text(`${key}: ${formattedValue}`, margin + 5, yPosition);
        yPosition += 5;
      }
      
      yPosition += 10;
    }
    
    // Add table data using autoTable
    if (data.headers && data.rows) {
      checkNewPage(50);
      
      // Prepare table data with German formatting
      const tableRows = data.rows.map((row: any[]) => 
        row.map((cell: any) => this.formatCellForGerman(cell, options))
      );
      
      pdf.autoTable({
        startY: yPosition,
        head: [data.headers],
        body: tableRows,
        theme: 'striped',
        styles: {
          fontSize: 9,
          cellPadding: 3,
          font: 'helvetica'
        },
        headStyles: {
          fillColor: [25, 118, 210], // Material Blue
          textColor: [255, 255, 255],
          fontStyle: 'bold'
        },
        alternateRowStyles: {
          fillColor: [248, 249, 250]
        },
        columnStyles: {
          // Right-align number columns
          1: { halign: 'right' }, // Amount columns
          3: { halign: 'right' }
        },
        margin: { left: margin, right: margin },
        didDrawPage: (data: any) => {
          // Add page numbers
          const pageNumber = (pdf as any).internal.getNumberOfPages();
          const currentPage = (pdf as any).internal.getCurrentPageInfo().pageNumber;
          pdf.setFontSize(8);
          pdf.setTextColor(128, 128, 128);
          pdf.text(
            `Seite ${currentPage} von ${pageNumber}`,
            pageWidth - margin - 30,
            pageHeight - 10
          );
        }
      });
      
      yPosition = (pdf as any).lastAutoTable.finalY + 20;
    }
    
    // Add summary
    if (data.summary) {
      checkNewPage(40);
      
      pdf.setFontSize(14);
      pdf.setTextColor(51, 51, 51);
      pdf.text('Zusammenfassung', margin, yPosition);
      yPosition += 8;
      
      // Draw summary box
      pdf.setDrawColor(224, 224, 224);
      pdf.setLineWidth(0.5);
      
      const summaryEntries = Object.entries(data.summary);
      const boxHeight = summaryEntries.length * 7 + 10;
      
      pdf.rect(margin, yPosition, pageWidth - 2 * margin, boxHeight);
      
      pdf.setFontSize(10);
      yPosition += 7;
      
      for (const [key, value] of summaryEntries) {
        const formattedValue = this.formatCellForGerman(value, options);
        pdf.text(`${key}:`, margin + 5, yPosition);
        pdf.text(formattedValue, pageWidth - margin - 5, yPosition, { align: 'right' });
        yPosition += 7;
      }
      
      yPosition += 10;
    }
    
    // Add footer
    checkNewPage(15);
    const timestamp = new Date().toLocaleString('de-DE', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
    
    pdf.setFontSize(8);
    pdf.setTextColor(128, 128, 128);
    pdf.text(`Erstellt am: ${timestamp}`, margin, pageHeight - 20);
    pdf.text('Generiert mit dem Deutschen Finanz-Dashboard', margin, pageHeight - 15);
    
    // Convert to blob
    const pdfBlob = pdf.output('blob');
    return pdfBlob;
  }
  
  /**
   * Format cell data for German locale
   */
  private formatCellForGerman(cell: any, options: ExportOptions): string {
    if (cell === null || cell === undefined) {
      return '';
    }
    
    // Handle dates
    if (cell instanceof Date || (typeof cell === 'string' && !isNaN(Date.parse(cell)))) {
      const date = cell instanceof Date ? cell : new Date(cell);
      return date.toLocaleDateString('de-DE', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      });
    }
    
    // Handle numbers (potential currency)
    if (typeof cell === 'number') {
      // Check if it looks like a currency amount (has decimal places or is large)
      if (cell % 1 !== 0 || cell > 999) {
        return new Intl.NumberFormat('de-DE', {
          style: 'currency',
          currency: 'EUR',
          minimumFractionDigits: 2,
          maximumFractionDigits: 2
        }).format(cell);
      } else {
        // Format as regular number with German locale
        return new Intl.NumberFormat('de-DE').format(cell);
      }
    }
    
    // Handle strings
    return cell.toString();
  }
  
  /**
   * Escape CSV row data
   */
  private escapeCSVRow(row: any[]): string {
    return row.map(cell => {
      const cellStr = cell?.toString() || '';
      // Escape quotes and wrap in quotes if contains comma, newline, or quote
      if (cellStr.includes(',') || cellStr.includes('"') || cellStr.includes('\n')) {
        return `"${cellStr.replace(/"/g, '""')}"`;
      }
      return cellStr;
    }).join(',');
  }
  
  /**
   * Format metadata for CSV
   */
  private formatMetadataForCSV(metadata: any): string {
    let content = '"Berichtsinformationen"\n';
    for (const [key, value] of Object.entries(metadata)) {
      content += `"${key}","${value}"\n`;
    }
    return content;
  }
  
  /**
   * Format summary data for CSV
   */
  private formatSummaryForCSV(summary: any, options: ExportOptions): string {
    let content = '"Zusammenfassung"\n';
    for (const [key, value] of Object.entries(summary)) {
      const formattedValue = this.formatCellForGerman(value, options);
      content += `"${key}","${formattedValue}"\n`;
    }
    return content;
  }
  
  /**
   * Download file to user's computer
   */
  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.style.display = 'none';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    // Clean up the URL object
    window.URL.revokeObjectURL(url);
  }
  
  /**
   * Export transaction data to CSV
   */
  exportTransactionsToCSV(
    transactions: any[],
    filename: string = 'transaktionen.csv'
  ): Promise<void> {
    const exportData: ExportData = {
      headers: [
        'Datum',
        'Kategorie',
        'Beschreibung',
        'Betrag',
        'MwSt. Satz',
        'MwSt. Betrag',
        'Rechnungsnummer',
        'Lieferant',
        'Zahlungsmethode'
      ],
      rows: transactions.map(t => [
        t.date,
        t.category,
        t.description,
        t.amount,
        t.taxRate ? `${t.taxRate}%` : '',
        t.vatAmount || '',
        t.invoiceNumber || '',
        t.supplier || '',
        t.paymentMethod || ''
      ]),
      title: 'Transaktions-Export',
      metadata: {
        'Anzahl Transaktionen': transactions.length,
        'Exportiert am': new Date()
      }
    };
    
    return this.downloadReport(exportData, filename, 'CSV');
  }
  
  /**
   * Export monthly report data
   */
  exportMonthlyReport(
    data: any,
    year: number,
    month: number,
    format: 'PDF' | 'CSV' | 'EXCEL'
  ): Promise<void> {
    const monthNames = [
      'Januar', 'Februar', 'März', 'April', 'Mai', 'Juni',
      'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'
    ];
    
    const filename = `Monatsbericht_${monthNames[month - 1]}_${year}.${format.toLowerCase()}`;
    
    const exportData: ExportData = {
      headers: ['Datum', 'Kategorie', 'Beschreibung', 'Betrag', 'MwSt.'],
      rows: data.transactions?.map((t: any) => [
        t.date,
        t.category,
        t.description,
        t.amount,
        t.vatAmount || ''
      ]) || [],
      title: `Monatsbericht ${monthNames[month - 1]} ${year}`,
      subtitle: 'Detaillierte Übersicht aller Transaktionen',
      metadata: {
        'Berichtszeitraum': `${monthNames[month - 1]} ${year}`,
        'Gesamteinnahmen': data.totalIncome || 0,
        'Gesamtausgaben': data.totalExpenses || 0,
        'Nettogewinn': data.netProfit || 0,
        'Anzahl Transaktionen': data.transactionCount || 0
      },
      summary: {
        'Gesamteinnahmen': data.totalIncome || 0,
        'Gesamtausgaben': data.totalExpenses || 0,
        'Nettogewinn': data.netProfit || 0
      }
    };
    
    return this.downloadReport(exportData, filename, format);
  }
  
  /**
   * Export yearly summary
   */
  exportYearlySummary(
    data: any,
    year: number,
    format: 'PDF' | 'CSV' | 'EXCEL'
  ): Promise<void> {
    const filename = `Jahresueberblick_${year}.${format.toLowerCase()}`;
    
    const exportData: ExportData = {
      headers: ['Kategorie', 'Betrag', 'Anzahl Transaktionen', 'Anteil'],
      rows: data.categoryBreakdown?.map((cat: any) => [
        cat.category,
        cat.totalAmount,
        cat.count,
        `${cat.percentage}%`
      ]) || [],
      title: `Jahresüberblick ${year}`,
      subtitle: 'Kategorienaufschlüsselung nach Ausgaben',
      metadata: {
        'Jahr': year,
        'Gesamtumsatz': data.totalIncome || 0,
        'Gesamtausgaben': data.totalExpenses || 0,
        'Gewinn/Verlust': data.netProfit || 0,
        'Anzahl Kategorien': data.categoryBreakdown?.length || 0
      },
      summary: {
        'Jährliche Einnahmen': data.totalIncome || 0,
        'Jährliche Ausgaben': data.totalExpenses || 0,
        'Jahresgewinn': data.netProfit || 0
      }
    };
    
    return this.downloadReport(exportData, filename, format);
  }
  
  /**
   * Export VAT report
   */
  exportVATReport(
    data: any,
    year: number,
    quarter: number,
    format: 'PDF' | 'CSV' | 'EXCEL'
  ): Promise<void> {
    const filename = `UStVA_${year}_Q${quarter}.${format.toLowerCase()}`;
    
    const exportData: ExportData = {
      headers: ['Position', 'Beschreibung', 'Betrag'],
      rows: [
        ['Umsätze 19%', 'Umsatzsteuerpflichtige Umsätze zum Steuersatz von 19%', data.sales19 || 0],
        ['USt 19%', 'Daraus zu entrichtende Umsatzsteuer', data.vat19 || 0],
        ['Umsätze 7%', 'Umsatzsteuerpflichtige Umsätze zum Steuersatz von 7%', data.sales7 || 0],
        ['USt 7%', 'Daraus zu entrichtende Umsatzsteuer', data.vat7 || 0],
        ['Vorsteuer', 'Abziehbare Vorsteuerbeträge', data.inputVat || 0],
        ['Zahllast', 'Umsatzsteuer-Zahllast bzw. -Erstattung', data.total || 0]
      ],
      title: `Umsatzsteuervoranmeldung Q${quarter} ${year}`,
      subtitle: 'Quartalsmeldung nach § 18 UStG',
      metadata: {
        'Berichtszeitraum': `${quarter}. Quartal ${year}`,
        'Anmeldeart': 'Quartalsmeldung',
        'Steuernummer': 'XX/XXX/XXXXX' // Placeholder
      },
      summary: {
        'Gesamte Steuerschuld 19%': data.vat19 || 0,
        'Gesamte Steuerschuld 7%': data.vat7 || 0,
        'Abzugsfähige Vorsteuer': data.inputVat || 0,
        'Zahllast/Erstattung': data.total || 0
      }
    };
    
    return this.downloadReport(exportData, filename, format);
  }
  
  /**
   * Get file extension for format
   */
  getFileExtension(format: 'PDF' | 'CSV' | 'EXCEL'): string {
    switch (format) {
      case 'PDF': return 'pdf';
      case 'CSV': return 'csv';
      case 'EXCEL': return 'xlsx';
      default: return 'txt';
    }
  }
  
  /**
   * Validate export data
   */
  validateExportData(data: any, format: string): { valid: boolean; message?: string } {
    if (!data) {
      return { valid: false, message: 'Keine Daten zum Exportieren vorhanden' };
    }
    
    if (!['PDF', 'CSV', 'EXCEL'].includes(format)) {
      return { valid: false, message: 'Ungültiges Exportformat' };
    }
    
    if (format === 'CSV' || format === 'EXCEL') {
      if (!data.headers || !Array.isArray(data.headers)) {
        return { valid: false, message: 'Fehlende oder ungültige Spaltenüberschriften' };
      }
      
      if (!data.rows || !Array.isArray(data.rows)) {
        return { valid: false, message: 'Fehlende oder ungültige Datenzeilen' };
      }
    }
    
    return { valid: true };
  }
}