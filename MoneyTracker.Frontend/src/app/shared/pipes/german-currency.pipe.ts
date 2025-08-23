import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'germanCurrency',
  standalone: true
})
export class GermanCurrencyPipe implements PipeTransform {
  transform(value: number | null | undefined, currency: string = 'EUR', showSymbol: boolean = true): string {
    if (value === null || value === undefined || isNaN(value)) {
      return showSymbol ? '0,00 €' : '0,00';
    }

    const formatted = new Intl.NumberFormat('de-DE', {
      style: showSymbol ? 'currency' : 'decimal',
      currency: currency,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);

    return formatted;
  }
}