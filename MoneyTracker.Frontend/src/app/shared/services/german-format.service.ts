import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GermanFormatService {

  constructor() { }

  /**
   * Formats a number as German currency (EUR)
   */
  formatCurrency(amount: number): string {
    if (amount === null || amount === undefined || isNaN(amount)) {
      return '0,00 €';
    }

    return new Intl.NumberFormat('de-DE', {
      style: 'currency',
      currency: 'EUR',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);
  }

  /**
   * Formats a date in German format (dd.MM.yyyy)
   */
  formatDate(date: Date): string {
    if (!date || !(date instanceof Date) || isNaN(date.getTime())) {
      return '';
    }

    return new Intl.DateTimeFormat('de-DE', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(date);
  }

  /**
   * Formats a date and time in German format (dd.MM.yyyy HH:mm)
   */
  formatDateTime(date: Date): string {
    if (!date || !(date instanceof Date) || isNaN(date.getTime())) {
      return '';
    }

    return new Intl.DateTimeFormat('de-DE', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  }

  /**
   * Formats a number using German locale (dot as thousands separator, comma as decimal)
   */
  formatNumber(number: number, decimalPlaces: number = 2): string {
    if (number === null || number === undefined || isNaN(number)) {
      return '0';
    }

    return new Intl.NumberFormat('de-DE', {
      minimumFractionDigits: decimalPlaces,
      maximumFractionDigits: decimalPlaces
    }).format(number);
  }

  /**
   * Formats a percentage using German locale
   */
  formatPercentage(percentage: number, decimalPlaces: number = 1): string {
    if (percentage === null || percentage === undefined || isNaN(percentage)) {
      return '0%';
    }

    return new Intl.NumberFormat('de-DE', {
      style: 'percent',
      minimumFractionDigits: decimalPlaces,
      maximumFractionDigits: decimalPlaces
    }).format(percentage);
  }

  /**
   * Parses a German formatted number string to a number
   * Examples: "1.234,56" -> 1234.56, "1,5" -> 1.5
   */
  parseGermanNumber(germanNumber: string): number {
    if (!germanNumber || typeof germanNumber !== 'string') {
      return 0;
    }

    // Remove thousands separators (dots) and replace decimal comma with dot
    const normalized = germanNumber
      .replace(/\./g, '') // Remove thousands separators
      .replace(',', '.'); // Replace decimal comma with dot

    const result = parseFloat(normalized);
    return isNaN(result) ? 0 : result;
  }

  /**
   * Parses a German formatted currency string to a number
   * Examples: "1.234,56 €" -> 1234.56, "€ 1,5" -> 1.5
   */
  parseGermanCurrency(germanCurrency: string): number {
    if (!germanCurrency || typeof germanCurrency !== 'string') {
      return 0;
    }

    // Remove currency symbols and spaces
    const numberPart = germanCurrency
      .replace(/€/g, '')
      .replace(/EUR/g, '')
      .trim();

    return this.parseGermanNumber(numberPart);
  }

  /**
   * Formats transaction data for API submission (converts German formats to API format)
   */
  formatTransactionForApi(transaction: any): any {
    if (!transaction) {
      return transaction;
    }

    const formatted = { ...transaction };

    // Convert amount if it's a string (German formatted)
    if (typeof formatted.amount === 'string') {
      formatted.amount = this.parseGermanNumber(formatted.amount);
    }

    // Convert date if it's a string
    if (typeof formatted.transactionDate === 'string') {
      formatted.transactionDate = new Date(formatted.transactionDate).toISOString();
    }

    // Handle VAT rates
    if (typeof formatted.defaultVatRate === 'string') {
      formatted.defaultVatRate = this.parseGermanNumber(formatted.defaultVatRate) / 100;
    }

    return formatted;
  }

  /**
   * Validates German IBAN format
   */
  validateGermanIBAN(iban: string): boolean {
    if (!iban) return false;
    
    // Remove spaces and convert to uppercase
    const cleanIban = iban.replace(/\s/g, '').toUpperCase();
    
    // German IBAN should start with DE and be 22 characters long
    const germanIbanPattern = /^DE\d{20}$/;
    return germanIbanPattern.test(cleanIban);
  }

  /**
   * Formats German IBAN with spaces for display
   */
  formatGermanIBAN(iban: string): string {
    if (!iban) return '';
    
    const cleanIban = iban.replace(/\s/g, '').toUpperCase();
    
    // Add spaces every 4 characters
    return cleanIban.replace(/(\w{4})/g, '$1 ').trim();
  }

  /**
   * Validates German BIC format
   */
  validateGermanBIC(bic: string): boolean {
    if (!bic) return false;
    
    // Remove spaces and convert to uppercase
    const cleanBic = bic.replace(/\s/g, '').toUpperCase();
    
    // BIC should be 8 or 11 characters
    const bicPattern = /^[A-Z]{4}DE[A-Z0-9]{2}([A-Z0-9]{3})?$/;
    return bicPattern.test(cleanBic);
  }

  /**
   * Formats file size in German locale
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return this.formatNumber(bytes / Math.pow(k, i), 2) + ' ' + sizes[i];
  }

  /**
   * Gets German month names
   */
  getGermanMonthNames(): string[] {
    return [
      'Januar', 'Februar', 'März', 'April', 'Mai', 'Juni',
      'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'
    ];
  }

  /**
   * Gets German day names
   */
  getGermanDayNames(): string[] {
    return ['Sonntag', 'Montag', 'Dienstag', 'Mittwoch', 'Donnerstag', 'Freitag', 'Samstag'];
  }

  /**
   * Gets German abbreviated day names
   */
  getGermanDayAbbreviations(): string[] {
    return ['So', 'Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa'];
  }
}
