# Transaction Management Frontend Components

This directory contains comprehensive transaction management UI components with German localization, built for Angular with Material Design.

## Components

### TransactionListComponent
**Purpose**: Display and manage lists of transactions with advanced filtering, sorting, and pagination.

**Features**:
- Paginated transaction display with German formatting
- Advanced filtering (date range, categories, amounts, search)
- Sortable columns with German locale
- Bulk selection and operations
- Mobile-responsive design
- Export functionality (CSV)
- Real-time search and filtering

**Usage**:
```html
<app-transaction-list
  [pageSize]="50"
  [allowBulkOperations]="true"
  (transactionSelected)="onTransactionSelected($event)"
  (bulkActionRequested)="onBulkAction($event)">
</app-transaction-list>
```

### TransactionFormComponent
**Purpose**: Create and edit transactions with comprehensive validation and German localization.

**Features**:
- Reactive forms with German validation messages
- Category selection with hierarchical display
- VAT calculation and display
- Date picker with German locale
- Currency input with proper formatting
- File upload integration for receipts
- Auto-suggestions for similar transactions
- Recurring transaction setup

**Usage**:
```html
<app-transaction-form
  [transaction]="selectedTransaction"
  [mode]="'edit'"
  (transactionSaved)="onTransactionSaved($event)"
  (cancelled)="onFormCancelled()">
</app-transaction-form>
```

### BulkUpdateDialogComponent
**Purpose**: Handle bulk operations on multiple transactions.

**Features**:
- Batch category assignment
- Bulk deletion with confirmation
- Mass field updates
- Progress tracking for operations
- German localized interface
- Validation for bulk operations

**Usage**:
```typescript
const dialogRef = this.dialog.open(BulkUpdateDialogComponent, {
  data: { selectedTransactions: this.selectedTransactions }
});
```

### PdfUploadComponent
**Purpose**: Upload PDF files (receipts, bank statements) with drag-and-drop functionality.

**Features**:
- Drag-and-drop file upload zone
- PDF validation and size checking
- Upload progress tracking
- File preview and management
- Batch upload capability
- German error messages and UI
- Integration with backend file storage

**Usage**:
```html
<app-pdf-upload
  [uploadType]="'receipt'"
  [maxFileSize]="10485760"
  [autoUpload]="false"
  (fileUploaded)="onFileUploaded($event)">
</app-pdf-upload>
```

### CategorySelectComponent
**Purpose**: Hierarchical category selection with multiple display modes.

**Features**:
- Tree view for category hierarchy
- Select dropdown mode
- Chip-based selection mode
- Search and filtering
- Multi-select capability
- German localization
- Color-coded categories

**Usage**:
```html
<app-category-select
  [displayMode]="'tree'"
  [allowMultiple]="false"
  [label]="'Kategorie auswählen'"
  (selectionChange)="onCategorySelected($event)">
</app-category-select>
```

## Services

### TransactionService
**Purpose**: Comprehensive transaction management with German formatting integration.

**Key Methods**:
- `getTransactions()` - Paginated transaction retrieval with filtering
- `createTransaction()` - Create new transactions
- `updateTransaction()` - Update existing transactions
- `bulkUpdateTransactions()` - Batch operations
- `getTransactionSummary()` - Analytics and reporting
- `exportToCSV()` - German-formatted CSV export
- `getCategorySuggestions()` - Smart categorization

### FileUploadService
**Purpose**: Handle PDF file uploads with progress tracking and validation.

**Key Methods**:
- `uploadFile()` - Single file upload with progress callbacks
- `uploadFiles()` - Batch file upload
- `validateFile()` - File validation with German error messages
- `downloadFile()` - File download functionality
- `processDocument()` - Extract data from uploaded documents

### GermanFormatService
**Purpose**: Comprehensive German localization and formatting utilities.

**Key Methods**:
- `formatCurrency()` - German currency formatting (€)
- `formatDate()` - German date formats (DD.MM.YYYY)
- `formatNumber()` - German number formatting (comma separator)
- `formatVAT()` - VAT display formatting
- `formatFileSize()` - File size in German format
- `parseNumber()` - Parse German-formatted numbers
- `validateIBAN()` - German IBAN validation

## Integration Notes

### Backend APIs
All components integrate with the following backend endpoints:
- `/api/transactions` - Transaction CRUD operations
- `/api/categories` - Category management
- `/api/files` - File upload and management
- Authentication via JWT tokens

### German Localization
- All dates formatted as DD.MM.YYYY
- Currency displayed as €X.XXX,XX
- Numbers use comma as decimal separator
- VAT calculations with German rates (7%, 19%)
- German validation messages throughout

### Mobile Responsiveness
- Responsive design for mobile devices
- Touch-friendly interfaces
- Adaptive layouts for different screen sizes
- Mobile-optimized file upload

### Accessibility
- WCAG compliance
- Keyboard navigation support
- Screen reader compatibility
- High contrast support
- Focus management

## Development Setup

1. **Dependencies**: All components use Angular Material and require these imports in your module:
   ```typescript
   import { MatTableModule } from '@angular/material/table';
   import { MatFormFieldModule } from '@angular/material/form-field';
   import { MatInputModule } from '@angular/material/input';
   import { MatSelectModule } from '@angular/material/select';
   import { MatDatepickerModule } from '@angular/material/datepicker';
   import { MatNativeDateModule } from '@angular/material/core';
   // ... other Material modules
   ```

2. **Environment Configuration**: Ensure your environment files include:
   ```typescript
   export const environment = {
     apiUrl: 'https://your-api-url.com/api',
     currency: 'EUR',
     defaultLanguage: 'de',
     // ... other config
   };
   ```

3. **Locale Setup**: Register German locale in your main.ts:
   ```typescript
   import { registerLocaleData } from '@angular/common';
   import localeDe from '@angular/common/locales/de';
   registerLocaleData(localeDe);
   ```

## Error Handling

All components include comprehensive error handling with:
- User-friendly German error messages
- Graceful degradation on API failures
- Loading states and progress indicators
- Validation error display
- Network error recovery

## Testing

Each component is designed with testability in mind:
- Modular architecture for unit testing
- Mock-friendly service injection
- Comprehensive type definitions
- Observable-based data flow
- Separation of concerns

## Performance

- Lazy loading of large data sets
- Virtual scrolling for large lists
- Optimized change detection
- Efficient file upload with chunking
- Caching strategies for category data