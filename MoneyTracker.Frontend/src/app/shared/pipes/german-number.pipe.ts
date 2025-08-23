import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'germanNumber',
  standalone: true
})
export class GermanNumberPipe implements PipeTransform {
  transform(value: number | null | undefined, minimumFractionDigits: number = 0, maximumFractionDigits: number = 2): string {
    if (value === null || value === undefined || isNaN(value)) {
      return '0';
    }

    return new Intl.NumberFormat('de-DE', {
      minimumFractionDigits,
      maximumFractionDigits
    }).format(value);
  }
}