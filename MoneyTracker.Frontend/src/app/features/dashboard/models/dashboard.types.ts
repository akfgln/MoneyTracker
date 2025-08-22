// Dashboard and reporting interfaces and types

// Dashboard Types
export interface DashboardSummary {
  totalBalance: number;
  monthlyIncome: number;
  monthlyExpenses: number;
  monthlyChange: number;
  incomeChange: number;
  expenseChange: number;
  monthlyVatPaid: number;
  monthlyVatDeductible: number;
}

export interface BudgetOverview {
  id: string;
  categoryId: string;
  categoryName: string;
  categoryColor?: string;
  categoryIcon?: string;
  limit: number;
  spent: number;
  percentage: number;
  remainingDays: number;
  isOverBudget: boolean;
}

export interface LegendItem {
  label: string;
  color: string;
  amount: number;
  percentage: number;
}

export interface MonthlyTrend {
  month: string;
  income: number;
  expenses: number;
  netAmount: number;
  year: number;
  monthNumber: number;
}

export interface CategorySpending {
  categoryId: string;
  categoryName: string;
  categoryColor?: string;
  categoryIcon?: string;
  amount: number;
  percentage: number;
  transactionCount: number;
  trend: 'up' | 'down' | 'stable';
  trendPercentage: number;
}

// Chart Types
export interface ChartData {
  labels?: string[];
  datasets?: ChartDataset[];
}

export interface ChartDataset {
  label: string;
  data: number[];
  backgroundColor?: string | string[];
  borderColor?: string | string[];
  borderWidth?: number;
  tension?: number;
  fill?: boolean;
  pointBackgroundColor?: string;
  pointBorderColor?: string;
  pointRadius?: number;
}

export interface ChartOptions {
  responsive?: boolean;
  maintainAspectRatio?: boolean;
  plugins?: {
    legend?: {
      display?: boolean;
      position?: 'top' | 'bottom' | 'left' | 'right';
      labels?: {
        usePointStyle?: boolean;
        font?: {
          size?: number;
          family?: string;
        };
      };
    };
    tooltip?: {
      callbacks?: {
        label?: (context: any) => string;
        title?: (context: any[]) => string;
      };
    };
  };
  scales?: {
    x?: {
      grid?: {
        display?: boolean;
        color?: string;
      };
      ticks?: {
        font?: {
          size?: number;
        };
      };
    };
    y?: {
      grid?: {
        display?: boolean;
        color?: string;
      };
      ticks?: {
        callback?: (value: any) => string;
        font?: {
          size?: number;
        };
      };
    };
  };
}

// Report Types
export interface VATSummary {
  startDate: Date;
  endDate: Date;
  vat19: number;
  vat7: number;
  vat0: number;
  totalVat: number;
  deductibleVat: number;
  netVat: number;
  totalGross: number;
  totalNet: number;
  vatTransactions: VATTransaction[];
}

export interface VATTransaction {
  id: string;
  date: Date;
  description: string;
  netAmount: number;
  vatRate: number;
  vatAmount: number;
  grossAmount: number;
  category: string;
  deductible: boolean;
}

export interface ReportHistoryItem {
  id: string;
  name: string;
  type: 'monthly' | 'yearly' | 'vat' | 'category' | 'custom';
  format: 'PDF' | 'CSV' | 'EXCEL';
  generatedDate: Date;
  fileSize: number;
  downloadUrl: string;
  parameters: ReportParameters;
}

export interface ReportParameters {
  startDate?: Date;
  endDate?: Date;
  year?: number;
  month?: number;
  categoryId?: string;
  includeVat?: boolean;
  includeAttachments?: boolean;
  groupBy?: 'category' | 'merchant' | 'date';
}

export interface CategoryGroup {
  label: string;
  categories: CategoryOption[];
}

export interface CategoryOption {
  id: string;
  name: string;
  color?: string;
  icon?: string;
  parentId?: string;
}

// Export Types
export interface ExportOptions {
  format: 'PDF' | 'CSV' | 'EXCEL';
  filename: string;
  includeCharts?: boolean;
  includeVatDetails?: boolean;
  germanFormatting?: boolean;
  pageOrientation?: 'portrait' | 'landscape';
  pageSize?: 'A4' | 'A3' | 'Letter';
}

export interface CSVExportData {
  headers: string[];
  rows: string[][];
  filename: string;
}

export interface PDFExportData {
  title: string;
  subtitle?: string;
  sections: PDFSection[];
  charts?: ChartImage[];
  footer?: string;
}

export interface PDFSection {
  title: string;
  content: string | TableData;
  type: 'text' | 'table' | 'chart';
}

export interface TableData {
  headers: string[];
  rows: (string | number)[][];
}

export interface ChartImage {
  title: string;
  dataUrl: string;
  width: number;
  height: number;
}

// Chart Color Schemes
export const GERMAN_CHART_COLORS = {
  primary: '#1976D2',
  secondary: '#424242',
  success: '#388E3C',
  warning: '#F57C00',
  error: '#D32F2F',
  info: '#0288D1',
  income: '#4CAF50',
  expense: '#F44336',
  neutral: '#9E9E9E',
  vat: '#FF9800',
  budget: '#9C27B0'
};

export const CATEGORY_COLORS = [
  '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0',
  '#9966FF', '#FF9F40', '#FF6384', '#C9CBCF',
  '#4BC0C0', '#FF6384', '#36A2EB', '#FFCE56'
];

// German Number Formats
export const GERMAN_FORMATS = {
  currency: {
    style: 'currency' as const,
    currency: 'EUR',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  },
  number: {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2
  },
  percentage: {
    style: 'percent' as const,
    minimumFractionDigits: 1,
    maximumFractionDigits: 1
  }
};

// German Month Names
export const GERMAN_MONTHS = [
  'Januar', 'Februar', 'März', 'April', 'Mai', 'Juni',
  'Juli', 'August', 'September', 'Oktober', 'November', 'Dezember'
];

export const GERMAN_MONTH_ABBREVIATIONS = [
  'Jan', 'Feb', 'Mär', 'Apr', 'Mai', 'Jun',
  'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dez'
];

// VAT Rates
export const GERMAN_VAT_RATES = {
  standard: 0.19,
  reduced: 0.07,
  zero: 0.00
};

// Report Templates
export interface ReportTemplate {
  id: string;
  name: string;
  description: string;
  type: 'monthly' | 'yearly' | 'vat' | 'category';
  sections: string[];
  germanTitle: string;
  germanDescription: string;
}