import { Injectable } from '@angular/core';
import { ChartOptions } from 'chart.js';

@Injectable({
  providedIn: 'root'
})
export class ChartConfigService {
  
  constructor() {}
  
  /**
   * Get German-formatted chart options for trend line charts
   */
  getGermanTrendChartOptions(): ChartOptions {
    return {
      responsive: true,
      maintainAspectRatio: false,
      interaction: {
        mode: 'index',
        intersect: false,
      },
      scales: {
        x: {
          display: true,
          title: {
            display: true,
            text: 'Monat'
          },
          grid: {
            display: false
          }
        },
        y: {
          display: true,
          title: {
            display: true,
            text: 'Betrag (€)'
          },
          ticks: {
            callback: function(value: any) {
              // Format numbers with German locale (comma as decimal separator)
              return new Intl.NumberFormat('de-DE', {
                style: 'currency',
                currency: 'EUR',
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
              }).format(value);
            }
          },
          grid: {
            color: 'rgba(0, 0, 0, 0.1)'
          }
        }
      },
      plugins: {
        legend: {
          display: true,
          position: 'top',
          labels: {
            usePointStyle: true,
            padding: 20
          }
        },
        tooltip: {
          enabled: true,
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          titleColor: '#ffffff',
          bodyColor: '#ffffff',
          borderColor: 'rgba(255, 255, 255, 0.2)',
          borderWidth: 1,
          cornerRadius: 6,
          displayColors: true,
          callbacks: {
            title: function(context: any) {
              return `Monat: ${context[0].label}`;
            },
            label: function(context: any) {
              const value = context.parsed.y;
              const formattedValue = new Intl.NumberFormat('de-DE', {
                style: 'currency',
                currency: 'EUR',
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
              }).format(value);
              return `${context.dataset.label}: ${formattedValue}`;
            },
            footer: function(context: any) {
              if (context.length === 2) {
                const income = context.find((c: any) => c.dataset.label === 'Einnahmen')?.parsed.y || 0;
                const expenses = context.find((c: any) => c.dataset.label === 'Ausgaben')?.parsed.y || 0;
                const profit = income - expenses;
                const formattedProfit = new Intl.NumberFormat('de-DE', {
                  style: 'currency',
                  currency: 'EUR',
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 2
                }).format(profit);
                return `Gewinn: ${formattedProfit}`;
              }
              return '';
            }
          }
        }
      },
      elements: {
        line: {
          tension: 0.4,
          borderWidth: 3
        },
        point: {
          radius: 6,
          hoverRadius: 8,
          borderWidth: 2,
          hoverBorderWidth: 3
        }
      }
    };
  }
  
  /**
   * Get German-formatted chart options for category doughnut charts
   */
  getGermanCategoryChartOptions(): ChartOptions {
    return {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '60%',
      plugins: {
        legend: {
          display: true,
          position: 'right',
          labels: {
            usePointStyle: true,
            padding: 15,
            font: {
              size: 12
            },
            generateLabels: function(chart: any) {
              const data = chart.data;
              if (data.labels.length && data.datasets.length) {
                const dataset = data.datasets[0];
                const total = dataset.data.reduce((sum: number, value: number) => sum + value, 0);
                
                return data.labels.map((label: string, index: number) => {
                  const value = dataset.data[index];
                  const percentage = ((value / total) * 100).toFixed(1);
                  const formattedValue = new Intl.NumberFormat('de-DE', {
                    style: 'currency',
                    currency: 'EUR',
                    minimumFractionDigits: 0,
                    maximumFractionDigits: 0
                  }).format(value);
                  
                  return {
                    text: `${label}: ${formattedValue} (${percentage}%)`,
                    fillStyle: dataset.backgroundColor[index],
                    strokeStyle: dataset.backgroundColor[index],
                    lineWidth: 0,
                    pointStyle: 'circle',
                    hidden: isNaN(value) || value === 0,
                    index: index
                  };
                });
              }
              return [];
            }
          }
        },
        tooltip: {
          enabled: true,
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          titleColor: '#ffffff',
          bodyColor: '#ffffff',
          borderColor: 'rgba(255, 255, 255, 0.2)',
          borderWidth: 1,
          cornerRadius: 6,
          displayColors: true,
          callbacks: {
            title: function(context: any) {
              return `Kategorie: ${context[0].label}`;
            },
            label: function(context: any) {
              const value = context.parsed;
              const total = context.dataset.data.reduce((sum: number, val: number) => sum + val, 0);
              const percentage = ((value / total) * 100).toFixed(1);
              const formattedValue = new Intl.NumberFormat('de-DE', {
                style: 'currency',
                currency: 'EUR',
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
              }).format(value);
              return `Betrag: ${formattedValue} (${percentage}%)`;
            }
          }
        }
      },
      animation: {
        animateRotate: true,
        animateScale: true,
        duration: 1000,
        easing: 'easeOutQuart'
      }
    };
  }
  
  /**
   * Get German-formatted chart options for bar charts
   */
  getGermanBarChartOptions(): ChartOptions {
    return {
      responsive: true,
      maintainAspectRatio: false,
      indexAxis: 'y', // Horizontal bars
      scales: {
        x: {
          display: true,
          title: {
            display: true,
            text: 'Betrag (€)'
          },
          ticks: {
            callback: function(value: any) {
              return new Intl.NumberFormat('de-DE', {
                style: 'currency',
                currency: 'EUR',
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
              }).format(value);
            }
          },
          grid: {
            color: 'rgba(0, 0, 0, 0.1)'
          }
        },
        y: {
          display: true,
          title: {
            display: true,
            text: 'Kategorie'
          },
          grid: {
            display: false
          }
        }
      },
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          enabled: true,
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          titleColor: '#ffffff',
          bodyColor: '#ffffff',
          borderColor: 'rgba(255, 255, 255, 0.2)',
          borderWidth: 1,
          cornerRadius: 6,
          callbacks: {
            title: function(context: any) {
              return `${context[0].label}`;
            },
            label: function(context: any) {
              const value = context.parsed.x;
              const formattedValue = new Intl.NumberFormat('de-DE', {
                style: 'currency',
                currency: 'EUR',
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
              }).format(value);
              return `Betrag: ${formattedValue}`;
            }
          }
        }
      },
      elements: {
        bar: {
          borderWidth: 0,
          borderRadius: 4
        }
      }
    };
  }
  
  /**
   * Get German-formatted chart options for polar area charts
   */
  getGermanPolarAreaChartOptions(): ChartOptions {
    return {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        r: {
          ticks: {
            callback: function(value: any) {
              return new Intl.NumberFormat('de-DE', {
                style: 'currency',
                currency: 'EUR',
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
              }).format(value);
            }
          },
          grid: {
            color: 'rgba(0, 0, 0, 0.1)'
          }
        }
      },
      plugins: {
        legend: {
          display: true,
          position: 'bottom',
          labels: {
            usePointStyle: true,
            padding: 20
          }
        },
        tooltip: {
          enabled: true,
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          titleColor: '#ffffff',
          bodyColor: '#ffffff',
          borderColor: 'rgba(255, 255, 255, 0.2)',
          borderWidth: 1,
          cornerRadius: 6,
          callbacks: {
            title: function(context: any) {
              return context[0].label;
            },
            label: function(context: any) {
              const value = context.parsed.r;
              const formattedValue = new Intl.NumberFormat('de-DE', {
                style: 'currency',
                currency: 'EUR',
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
              }).format(value);
              return `Betrag: ${formattedValue}`;
            }
          }
        }
      }
    };
  }
  
  /**
   * Format number with German locale
   */
  formatGermanCurrency(value: number, options?: {
    minimumFractionDigits?: number;
    maximumFractionDigits?: number;
  }): string {
    return new Intl.NumberFormat('de-DE', {
      style: 'currency',
      currency: 'EUR',
      minimumFractionDigits: options?.minimumFractionDigits ?? 2,
      maximumFractionDigits: options?.maximumFractionDigits ?? 2
    }).format(value);
  }
  
  /**
   * Format percentage with German locale
   */
  formatGermanPercentage(value: number, options?: {
    minimumFractionDigits?: number;
    maximumFractionDigits?: number;
  }): string {
    return new Intl.NumberFormat('de-DE', {
      style: 'percent',
      minimumFractionDigits: options?.minimumFractionDigits ?? 1,
      maximumFractionDigits: options?.maximumFractionDigits ?? 1
    }).format(value / 100);
  }
  
  /**
   * Format date with German locale
   */
  formatGermanDate(date: Date, options?: {
    year?: 'numeric' | '2-digit';
    month?: 'numeric' | '2-digit' | 'long' | 'short' | 'narrow';
    day?: 'numeric' | '2-digit';
  }): string {
    return new Intl.DateTimeFormat('de-DE', {
      year: options?.year ?? 'numeric',
      month: options?.month ?? '2-digit',
      day: options?.day ?? '2-digit'
    }).format(date);
  }
  
  /**
   * Get common German color palette for charts
   */
  getGermanColorPalette(): string[] {
    return [
      '#FF6384', // Rosa
      '#36A2EB', // Blau
      '#FFCE56', // Gelb
      '#4BC0C0', // Türkis
      '#9966FF', // Lila
      '#FF9F40', // Orange
      '#FF6384', // Rosa (repeat)
      '#C9CBCF', // Grau
      '#4BC0C0', // Türkis (repeat)
      '#FF6384'  // Rosa (repeat)
    ];
  }
  
  /**
   * Get German months array
   */
  getGermanMonths(): string[] {
    return [
      'Januar', 'Februar', 'März', 'April', 'Mai', 'Juni',
      'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'
    ];
  }
  
  /**
   * Get German short months array
   */
  getGermanShortMonths(): string[] {
    return [
      'Jan', 'Feb', 'Mär', 'Apr', 'Mai', 'Jun',
      'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dez'
    ];
  }
  
  /**
   * Get German day names
   */
  getGermanDays(): string[] {
    return [
      'Sonntag', 'Montag', 'Dienstag', 'Mittwoch',
      'Donnerstag', 'Freitag', 'Samstag'
    ];
  }
  
  /**
   * Get German short day names
   */
  getGermanShortDays(): string[] {
    return ['So', 'Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa'];
  }
}