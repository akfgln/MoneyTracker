// Environment configuration for development
export const environment = {
  production: false,
  apiUrl: 'http://localhost:3000/api',
  reportApiUrl: 'http://localhost:3000/api/reports',
  enableMockData: true, // Use mock data when API is unavailable
  enableConsoleLogging: true,
  version: '1.0.0-dev',
  
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
    maxFileSize: 10 * 1024 * 1024, // 10MB
    allowedFormats: ['PDF', 'CSV', 'EXCEL'],
    defaultFormat: 'PDF' as const
  },
  
  // API timeout and retry settings
  api: {
    timeout: 30000, // 30 seconds
    retries: 3,
    retryDelay: 1000 // 1 second
  }
};