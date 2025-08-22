// Environment configuration for production
export const environment = {
  production: true,
  apiUrl: 'https://api.your-domain.com/api',
  reportApiUrl: 'https://api.your-domain.com/api/reports',
  enableMockData: false, // Disable mock data in production
  enableConsoleLogging: false,
  version: '1.0.0',
  
  // German locale settings
  locale: 'de-DE',
  currency: 'EUR',
  dateFormat: 'dd.MM.yyyy',
  
  // Chart.js configuration
  chartDefaults: {
    responsive: true,
    maintainAspectRatio: false,
    locale: 'de-DE'
  },
  
  // File export settings
  export: {
    maxFileSize: 50 * 1024 * 1024, // 50MB for production
    allowedFormats: ['PDF', 'CSV', 'EXCEL'],
    defaultFormat: 'PDF' as const
  },
  
  // API timeout and retry settings
  api: {
    timeout: 45000, // 45 seconds for production
    retries: 5,
    retryDelay: 2000 // 2 seconds
  }
};