import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'fileSize',
  standalone: true
})
export class FileSizePipe implements PipeTransform {
  transform(bytes: number | null | undefined): string {
    if (bytes === null || bytes === undefined || isNaN(bytes) || bytes === 0) {
      return '0 Bytes';
    }

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    const size = bytes / Math.pow(k, i);
    const formattedSize = new Intl.NumberFormat('de-DE', {
      minimumFractionDigits: i === 0 ? 0 : 2,
      maximumFractionDigits: 2
    }).format(size);

    return `${formattedSize} ${sizes[i]}`;
  }
}
