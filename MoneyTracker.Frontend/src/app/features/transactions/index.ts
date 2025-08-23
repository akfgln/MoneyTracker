// Pages
export { TransactionManagementComponent } from './pages/transaction-management.component';

// Components
export { TransactionListComponent } from './components/transaction-list/transaction-list.component';
export { TransactionFormComponent } from './components/transaction-form/transaction-form.component';
export { BulkUpdateDialogComponent } from './components/bulk-update-dialog/bulk-update-dialog.component';
export { PdfUploadComponent } from './components/pdf-upload/pdf-upload.component';
export { CategorySelectComponent } from './components/category-select/category-select.component';

// Services
export { TransactionService } from './services/transaction.service';
export { FileUploadService } from './services/file-upload.service';
export { GermanFormatService } from './services/german-format.service';

// Re-export commonly used types
export type {
  Transaction,
  Category,
  TransactionFilter,
  TransactionSummary,
  CategorySummary,
  MonthlyTrend,
  BulkUpdateData,
  TransactionPagedResult,
  RecurringPattern
} from './services/transaction.service';

export type {
  FileUploadResult,
  UploadProgress,
  //UploadedFile
} from './services/file-upload.service';

export type {
  GermanFormatOptions
} from './services/german-format.service';