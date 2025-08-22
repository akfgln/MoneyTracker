import { Injectable } from '@angular/core';
import {
  ChartOptions,
  ChartData,
  MonthlyTrend,
  CategorySpending,
  GERMAN_CHART_COLORS,
  CATEGORY_COLORS,
  GERMAN_MONTHS,
  GERMAN_MONTH_ABBREVIATIONS
} from '../models/dashboard.types';

@Injectable({
  providedIn: 'root'
})
export class ChartConfigService {
  private readonly locale = 'de-DE';

  constructor() {}

  /**
   * Get German-formatted trend chart options
   */
  getGermanTrendChartOptions(): ChartOptions {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: 'top',
          labels: {
            usePointStyle: true,
            font: {
              size: 12,
              family: 'Roboto, "Helvetica Neue", sans-serif'
            }
          }
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const label = context.dataset.label || '';
              const value = this.formatCurrency(context.parsed.y);
              return `${label}: ${value}`;
            },
            title: (context) => {
              return context[0]?.label || '';
            }
          }
        }
      },
      scales: {
        x: {
          grid: {
            display: true,
            color: '#f0f0f0'
          },
          ticks: {
            font: {
              size: 11
            }
          }
        },
        y: {
          grid: {
            display: true,
            color: '#f0f0f0'
          },
          ticks: {
            callback: (value) => {
              return this.formatCurrency(value as number, true);
            },
            font: {
              size: 11
            }
          }
        }
      }
    };
  }

  /**
   * Get German-formatted category chart options
   */
  getGermanCategoryChartOptions(totalAmount: number): ChartOptions {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false // We'll use custom legend
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const label = context.label || '';
              const value = this.formatCurrency(context.parsed as number);
              const percentage = ((context.parsed as number / totalAmount) * 100).toFixed(1);
              return `${label}: ${value} (${percentage}%)`;
            }
          }
        }
      }
    };
  }

  /**
   * Create trend chart data from monthly trends
   */
  createTrendChartData(monthlyTrends: MonthlyTrend[]): ChartData {
    const labels = monthlyTrends.map(trend => 
      GERMAN_MONTH_ABBREVIATIONS[trend.monthNumber - 1]
    );

    return {
      labels,
      datasets: [
        {
          label: 'Einnahmen',
          data: monthlyTrends.map(trend => trend.income),
          backgroundColor: GERMAN_CHART_COLORS.income + '20',
          borderColor: GERMAN_CHART_COLORS.income,
          borderWidth: 2,
          tension: 0.4,
          fill: true,
          pointBackgroundColor: GERMAN_CHART_COLORS.income,
          pointBorderColor: '#ffffff',
          pointRadius: 4
        },
        {
          label: 'Ausgaben',
          data: monthlyTrends.map(trend => Math.abs(trend.expenses)),
          backgroundColor: GERMAN_CHART_COLORS.expense + '20',
          borderColor: GERMAN_CHART_COLORS.expense,
          borderWidth: 2,
          tension: 0.4,
          fill: true,
          pointBackgroundColor: GERMAN_CHART_COLORS.expense,
          pointBorderColor: '#ffffff',
          pointRadius: 4
        },
        {
          label: 'Saldo',
          data: monthlyTrends.map(trend => trend.netAmount),
          backgroundColor: GERMAN_CHART_COLORS.primary + '20',
          borderColor: GERMAN_CHART_COLORS.primary,
          borderWidth: 3,
          tension: 0.4,
          fill: false,
          pointBackgroundColor: GERMAN_CHART_COLORS.primary,
          pointBorderColor: '#ffffff',
          pointRadius: 5
        }
      ]
    };
  }

  /**
   * Create category chart data from category spending
   */
  createCategoryChartData(categorySpending: CategorySpending[]): ChartData {
    // Sort by amount descending and take top 10
    const topCategories = categorySpending
      .sort((a, b) => b.amount - a.amount)
      .slice(0, 10);

    const labels = topCategories.map(cat => cat.categoryName);
    const data = topCategories.map(cat => Math.abs(cat.amount));
    const colors = topCategories.map((cat, index) => 
      cat.categoryColor || CATEGORY_COLORS[index % CATEGORY_COLORS.length]
    );

    return {
      labels,
      datasets: [
        {
          label: 'Ausgaben nach Kategorie',
          data,
          backgroundColor: colors,
          borderColor: colors.map(color => this.darkenColor(color, 0.1)),
          borderWidth: 2
        }
      ]
    };
  }

  /**
   * Create budget progress chart data
   */
  createBudgetChartData(budgets: any[]): ChartData {
    const labels = budgets.map(budget => budget.categoryName);
    const spentData = budgets.map(budget => budget.spent);
    const remainingData = budgets.map(budget => 
      Math.max(0, budget.limit - budget.spent)
    );

    return {
      labels,
      datasets: [
        {
          label: 'Ausgegeben',
          data: spentData,
          backgroundColor: GERMAN_CHART_COLORS.expense + '80',
          borderColor: GERMAN_CHART_COLORS.expense,
          borderWidth: 1
        },
        {
          label: 'Verfügbar',
          data: remainingData,
          backgroundColor: GERMAN_CHART_COLORS.success + '80',
          borderColor: GERMAN_CHART_COLORS.success,
          borderWidth: 1
        }
      ]
    };
  }

  /**
   * Create VAT breakdown chart
   */
  createVatChartData(vat19: number, vat7: number, vat0: number): ChartData {
    const data = [Math.abs(vat19), Math.abs(vat7), Math.abs(vat0)];
    const labels = ['MwSt. 19%', 'MwSt. 7%', 'MwSt. 0%'];
    const colors = [
      GERMAN_CHART_COLORS.error,
      GERMAN_CHART_COLORS.warning,
      GERMAN_CHART_COLORS.neutral
    ];

    return {
      labels,
      datasets: [
        {
          label: 'MwSt.-Aufschlüsselung',
          data,
          backgroundColor: colors,
          borderColor: colors.map(color => this.darkenColor(color, 0.1)),
          borderWidth: 2
        }
      ]
    };
  }

  /**
   * Create income vs expense comparison chart
   */
  createComparisonChartData(
    currentIncome: number,
    currentExpenses: number,
    previousIncome: number,
    previousExpenses: number
  ): ChartData {
    return {
      labels: ['Dieser Monat', 'Letzter Monat'],
      datasets: [
        {
          label: 'Einnahmen',
          data: [currentIncome, previousIncome],
          backgroundColor: GERMAN_CHART_COLORS.income + '80',
          borderColor: GERMAN_CHART_COLORS.income,
          borderWidth: 2
        },
        {
          label: 'Ausgaben',
          data: [Math.abs(currentExpenses), Math.abs(previousExpenses)],
          backgroundColor: GERMAN_CHART_COLORS.expense + '80',
          borderColor: GERMAN_CHART_COLORS.expense,
          borderWidth: 2
        }
      ]
    };
  }

  /**
   * Generate chart color palette
   */
  generateColorPalette(count: number): string[] {
    const colors = [];
    for (let i = 0; i < count; i++) {
      const hue = (i * 360 / count) % 360;
      colors.push(`hsl(${hue}, 70%, 60%)`);
    }
    return colors;
  }

  /**
   * Darken a color by a specified amount
   */
  private darkenColor(color: string, amount: number): string {
    // Simple color darkening - in production you might want to use a color library
    if (color.startsWith('#')) {
      const num = parseInt(color.slice(1), 16);
      const r = Math.max(0, (num >> 16) - Math.round(255 * amount));
      const g = Math.max(0, ((num >> 8) & 0x00FF) - Math.round(255 * amount));
      const b = Math.max(0, (num & 0x0000FF) - Math.round(255 * amount));
      return `#${((r << 16) | (g << 8) | b).toString(16).padStart(6, '0')}`;
    }
    return color;
  }

  /**
   * Format currency for German locale
   */
  private formatCurrency(value: number, short: boolean = false): string {
    const formatter = new Intl.NumberFormat(this.locale, {
      style: 'currency',
      currency: 'EUR',
      minimumFractionDigits: short ? 0 : 2,
      maximumFractionDigits: short ? 0 : 2
    });
    return formatter.format(value);
  }

  /**
   * Get chart configuration for mobile devices
   */
  getMobileChartOptions(baseOptions: ChartOptions): ChartOptions {
    return {
      ...baseOptions,
      plugins: {
        ...baseOptions.plugins,
        legend: {
          ...baseOptions.plugins?.legend,
          labels: {
            ...baseOptions.plugins?.legend?.labels,
            font: {
              size: 10
            }
          }
        }
      },
      scales: {
        x: {
          ...baseOptions.scales?.x,
          ticks: {
            ...baseOptions.scales?.x?.ticks,
            font: {
              size: 9
            }
          }
        },
        y: {
          ...baseOptions.scales?.y,
          ticks: {
            ...baseOptions.scales?.y?.ticks,
            font: {
              size: 9
            }
          }
        }
      }
    };
  }

  /**
   * Export chart as base64 image
   */
  exportChartAsImage(chartElement: any): string {
    return chartElement.toBase64Image('image/png', 1);
  }
}