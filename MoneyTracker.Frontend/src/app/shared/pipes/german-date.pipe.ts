import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'germanDate',
  standalone: true
})
export class GermanDatePipe implements PipeTransform {
  transform(value: Date | string | null | undefined, format: 'short' | 'medium' | 'long' | 'full' | 'custom' = 'short', customFormat?: string): string {
    if (!value) {
      return '';
    }

    const date = value instanceof Date ? value : new Date(value);
    
    if (isNaN(date.getTime())) {
      return '';
    }

    switch (format) {
      case 'short':
        return new Intl.DateTimeFormat('de-DE').format(date); // DD.MM.YYYY
      case 'medium':
        return new Intl.DateTimeFormat('de-DE', {
          year: 'numeric',
          month: 'short',
          day: 'numeric'
        }).format(date); // D. MMM YYYY
      case 'long':
        return new Intl.DateTimeFormat('de-DE', {
          year: 'numeric',
          month: 'long',
          day: 'numeric'
        }).format(date); // D. MMMM YYYY
      case 'full':
        return new Intl.DateTimeFormat('de-DE', {
          weekday: 'long',
          year: 'numeric',
          month: 'long',
          day: 'numeric'
        }).format(date); // Weekday, D. MMMM YYYY
      case 'custom':
        if (customFormat) {
          return this.formatCustom(date, customFormat);
        }
        return new Intl.DateTimeFormat('de-DE').format(date);
      default:
        return new Intl.DateTimeFormat('de-DE').format(date);
    }
  }

  private formatCustom(date: Date, format: string): string {
    const map: { [key: string]: string } = {
      'DD': String(date.getDate()).padStart(2, '0'),
      'D': String(date.getDate()),
      'MM': String(date.getMonth() + 1).padStart(2, '0'),
      'M': String(date.getMonth() + 1),
      'YYYY': String(date.getFullYear()),
      'YY': String(date.getFullYear()).slice(-2),
      'HH': String(date.getHours()).padStart(2, '0'),
      'H': String(date.getHours()),
      'mm': String(date.getMinutes()).padStart(2, '0'),
      'm': String(date.getMinutes()),
      'ss': String(date.getSeconds()).padStart(2, '0'),
      's': String(date.getSeconds())
    };

    return format.replace(/DD|D|MM|M|YYYY|YY|HH|H|mm|m|ss|s/g, matched => map[matched] || matched);
  }
}