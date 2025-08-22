import { Injectable } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeDe from '@angular/common/locales/de';

// Register German locale
registerLocaleData(localeDe);

export interface GermanFormatOptions {
  locale?: string;
  currency?: string;
  minimumFractionDigits?: number;
  maximumFractionDigits?: number;
}

@Injectable({
  providedIn: 'root'
})
export class GermanFormatService {
  private readonly defaultLocale = 'de-DE';
  private readonly defaultCurrency = 'EUR';
  
  constructor() {}
  
  /**
   * Format currency amount in German format
   * @param amount The amount to format
   * @param options Formatting options
   * @returns Formatted currency string
   */
  formatCurrency(
    amount: number, 
    options: GermanFormatOptions = {}
  ): string {
    const locale = options.locale || this.defaultLocale;
    const currency = options.currency || this.defaultCurrency;
    
    return new Intl.NumberFormat(locale, {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: options.minimumFractionDigits ?? 2,
      maximumFractionDigits: options.maximumFractionDigits ?? 2
    }).format(amount);
  }
  
  /**
   * Format number in German format
   * @param value The number to format
   * @param options Formatting options
   * @returns Formatted number string
   */
  formatNumber(
    value: number, 
    options: GermanFormatOptions = {}
  ): string {
    const locale = options.locale || this.defaultLocale;
    
    return new Intl.NumberFormat(locale, {
      minimumFractionDigits: options.minimumFractionDigits ?? 0,
      maximumFractionDigits: options.maximumFractionDigits ?? 2
    }).format(value);
  }
  
  /**
   * Format percentage in German format
   * @param value The percentage value (0.15 for 15%)
   * @param options Formatting options
   * @returns Formatted percentage string
   */
  formatPercentage(
    value: number, 
    options: GermanFormatOptions = {}
  ): string {
    const locale = options.locale || this.defaultLocale;
    
    return new Intl.NumberFormat(locale, {
      style: 'percent',
      minimumFractionDigits: options.minimumFractionDigits ?? 0,
      maximumFractionDigits: options.maximumFractionDigits ?? 2
    }).format(value);
  }
  
  /**
   * Format date in German format
   * @param date The date to format
   * @param style The date style ('short', 'medium', 'long', 'full')
   * @param locale Optional locale override
   * @returns Formatted date string
   */
  formatDate(
    date: Date | string | number,
    style: 'short' | 'medium' | 'long' | 'full' = 'medium',
    locale?: string
  ): string {
    const dateObj = new Date(date);
    const loc = locale || this.defaultLocale;
    
    const options: Intl.DateTimeFormatOptions = {
      dateStyle: style
    };
    
    return new Intl.DateTimeFormat(loc, options).format(dateObj);
  }
  
  /**
   * Format date and time in German format
   * @param date The date to format
   * @param dateStyle The date style
   * @param timeStyle The time style
   * @param locale Optional locale override
   * @returns Formatted date and time string
   */
  formatDateTime(
    date: Date | string | number,
    dateStyle: 'short' | 'medium' | 'long' | 'full' = 'medium',
    timeStyle: 'short' | 'medium' | 'long' | 'full' = 'short',
    locale?: string
  ): string {
    const dateObj = new Date(date);
    const loc = locale || this.defaultLocale;
    
    const options: Intl.DateTimeFormatOptions = {
      dateStyle,
      timeStyle
    };
    
    return new Intl.DateTimeFormat(loc, options).format(dateObj);
  }
  
  /**
   * Format time in German format
   * @param date The date/time to format
   * @param style The time style
   * @param locale Optional locale override
   * @returns Formatted time string
   */
  formatTime(
    date: Date | string | number,
    style: 'short' | 'medium' | 'long' | 'full' = 'short',
    locale?: string
  ): string {
    const dateObj = new Date(date);
    const loc = locale || this.defaultLocale;
    
    const options: Intl.DateTimeFormatOptions = {
      timeStyle: style
    };
    
    return new Intl.DateTimeFormat(loc, options).format(dateObj);
  }
  
  /**
   * Format file size in German format
   * @param bytes File size in bytes
   * @param precision Number of decimal places
   * @returns Formatted file size string
   */
  formatFileSize(bytes: number, precision: number = 1): string {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    const size = bytes / Math.pow(k, i);
    
    // Use German number formatting
    const formattedSize = this.formatNumber(size, {
      maximumFractionDigits: precision
    });
    
    return `${formattedSize} ${sizes[i]}`;
  }
  
  /**
   * Parse German formatted number string to number
   * @param value The formatted number string
   * @returns Parsed number
   */
  parseNumber(value: string): number {
    if (!value || typeof value !== 'string') {
      return 0;
    }
    
    // Remove spaces and replace German decimal separator
    const cleanValue = value
      .replace(/\s/g, '') // Remove spaces
      .replace(/\./g, '') // Remove thousands separator
      .replace(',', '.'); // Replace decimal separator
    
    return parseFloat(cleanValue) || 0;
  }
  
  /**
   * Parse German formatted currency string to number
   * @param value The formatted currency string
   * @returns Parsed number
   */
  parseCurrency(value: string): number {
    if (!value || typeof value !== 'string') {
      return 0;
    }
    
    // Remove currency symbol, spaces, and format
    const cleanValue = value
      .replace(/[€$£¥]/g, '') // Remove currency symbols
      .replace(/\s/g, '') // Remove spaces
      .replace(/\./g, '') // Remove thousands separator
      .replace(',', '.'); // Replace decimal separator
    
    return parseFloat(cleanValue) || 0;
  }
  
  /**
   * Format German VAT (Mehrwertsteuer) display
   * @param vatRate VAT rate as decimal (0.19 for 19%)
   * @param amount The amount before VAT
   * @returns Formatted VAT information
   */
  formatVAT(vatRate: number, amount: number): {
    rate: string;
    amount: string;
    vatAmount: string;
    totalAmount: string;
  } {
    const vatAmount = amount * vatRate;
    const totalAmount = amount + vatAmount;
    
    return {
      rate: this.formatPercentage(vatRate),
      amount: this.formatCurrency(amount),
      vatAmount: this.formatCurrency(vatAmount),
      totalAmount: this.formatCurrency(totalAmount)
    };
  }
  
  /**
   * Format German postal code
   * @param postalCode The postal code
   * @returns Formatted postal code
   */
  formatPostalCode(postalCode: string | number): string {
    const code = postalCode.toString().padStart(5, '0');
    return code;
  }
  
  /**
   * Format German phone number
   * @param phoneNumber The phone number
   * @returns Formatted phone number
   */
  formatPhoneNumber(phoneNumber: string): string {
    // Remove all non-numeric characters
    const cleaned = phoneNumber.replace(/\D/g, '');
    
    // German phone number formatting
    if (cleaned.startsWith('49')) {
      // International format
      const number = cleaned.substring(2);
      if (number.length >= 10) {
        return `+49 ${number.substring(0, 3)} ${number.substring(3, 6)} ${number.substring(6)}`;
      }
    } else if (cleaned.startsWith('0')) {
      // National format
      if (cleaned.length >= 10) {
        return `${cleaned.substring(0, 4)} ${cleaned.substring(4, 7)} ${cleaned.substring(7)}`;
      }
    }
    
    return phoneNumber; // Return original if formatting fails
  }
  
  /**
   * Validate German IBAN
   * @param iban The IBAN to validate
   * @returns Validation result
   */
  validateIBAN(iban: string): { isValid: boolean; formatted?: string } {
    if (!iban) {
      return { isValid: false };
    }
    
    // Remove spaces and convert to uppercase
    const cleanIban = iban.replace(/\s/g, '').toUpperCase();
    
    // Check if it's a German IBAN
    if (!cleanIban.startsWith('DE') || cleanIban.length !== 22) {
      return { isValid: false };
    }
    
    // Format IBAN with spaces for display
    const formatted = cleanIban.replace(/(....)/g, '$1 ').trim();
    
    // Basic IBAN checksum validation would go here
    // For now, just validate format
    const isValid = /^DE\d{20}$/.test(cleanIban);
    
    return { isValid, formatted };
  }
  
  /**
   * Get German month names
   * @param format 'long' or 'short'
   * @returns Array of month names
   */
  getMonthNames(format: 'long' | 'short' = 'long'): string[] {
    const formatter = new Intl.DateTimeFormat(this.defaultLocale, {
      month: format
    });
    
    return Array.from({ length: 12 }, (_, i) => 
      formatter.format(new Date(2023, i, 1))
    );
  }
  
  /**
   * Get German day names
   * @param format 'long' or 'short'
   * @returns Array of day names
   */
  getDayNames(format: 'long' | 'short' = 'long'): string[] {
    const formatter = new Intl.DateTimeFormat(this.defaultLocale, {
      weekday: format
    });
    
    // Start with Monday (German week starts on Monday)
    return Array.from({ length: 7 }, (_, i) => 
      formatter.format(new Date(2023, 0, 2 + i)) // January 2, 2023 was a Monday
    );
  }
}