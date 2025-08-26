import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'germanNumber',
  standalone: true
})
export class GermanNumberPipe implements PipeTransform {
  transform(value: number | null | undefined, digitsInfo?: string): string {
    if (value === null || value === undefined || isNaN(value)) {
      return '0';
    }

    let minimumFractionDigits = 0;
    let maximumFractionDigits = 2;
    
    // Parse Angular decimal pipe format like '1.0-1', '1.2-2', etc.
    if (digitsInfo) {
      const parts = digitsInfo.split('.');
      if (parts.length === 2) {
        const fractionParts = parts[1].split('-');
        if (fractionParts.length === 2) {
          minimumFractionDigits = parseInt(fractionParts[0]) || 0;
          maximumFractionDigits = parseInt(fractionParts[1]) || 2;
        }
      }
    }

    return new Intl.NumberFormat('de-DE', {
      minimumFractionDigits,
      maximumFractionDigits
    }).format(value);
  }
}