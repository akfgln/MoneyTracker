export enum TransactionType {
  Income = 'Income',
  Expense = 'Expense',
  Transfer = 'Transfer'
}

export enum CategoryType {
  Income = 'Income',
  Expense = 'Expense'
}

export enum FileStatus {
  Uploading = 'Uploading',
  Processing = 'Processing',
  Processed = 'Processed',
  Failed = 'Failed'
}

export interface Transaction {
  id: string;
  userId: string;
  accountId: string;
  categoryId: string;
  amount: number;
  netAmount: number;
  vatAmount: number;
  vatRate: number;
  description: string;
  merchantName?: string;
  transactionDate: Date;
  transactionType: TransactionType;
  notes?: string;
  tags?: string[];
  referenceNumber?: string;
  uploadedFileId?: string;
  isRecurring: boolean;
  recurringFrequency?: string;
  recurringEndDate?: Date;
  isReconciled: boolean;
  reconciledDate?: Date;
  createdAt: Date;
  updatedAt: Date;
  
  // Navigation properties
  category?: Category;
  account?: Account;
  uploadedFile?: UploadedFile;
}

export interface Category {
  id: string;
  userId: string;
  name: string;
  description?: string;
  categoryType: CategoryType;
  icon?: string;
  color?: string;
  parentCategoryId?: string;
  defaultVatRate: number;
  keywords?: string;
  isSystemCategory: boolean;
  isActive: boolean;
  sortOrder: number;
  createdAt: Date;
  updatedAt: Date;
  
  // Navigation properties
  parentCategory?: Category;
  subCategories?: Category[];
  transactions?: Transaction[];
  
  // Computed properties
  transactionCount?: number;
  totalAmount?: number;
}

export interface Account {
  id: string;
  userId: string;
  accountName: string;
  accountType: string;
  iban?: string;
  bic?: string;
  bankName?: string;
  currency: string;
  balance: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface UploadedFile {
  id: string;
  userId: string;
  fileName: string;
  originalFileName: string;
  fileSize: number;
  fileType: string;
  filePath: string;
  fileUrl?: string;
  uploadDate: Date;
  status: FileStatus;
  processingMessage?: string;
  extractedData?: any;
  virusScanResult?: string;
  virusScanDate?: Date;
  downloadCount: number;
  lastDownloadDate?: Date;
  expiryDate?: Date;
  isExpired: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface VATCalculation {
  grossAmount: number;
  netAmount: number;
  vatAmount: number;
  vatRate: number;
}

export interface CategorySuggestion {
  categoryId: string;
  categoryName: string;
  categoryIcon?: string;
  categoryColor?: string;
  confidenceScore: number;
  matchReason: string;
}

export interface CategoryGroup {
  label: string;
  categoryType: CategoryType;
  categories: Category[];
}

// Request DTOs
export interface CreateTransactionDto {
  accountId: string;
  categoryId?: string;
  amount: number;
  description: string;
  merchantName?: string;
  transactionDate: Date;
  transactionType: TransactionType;
  notes?: string;
  tags?: string[];
  referenceNumber?: string;
  isRecurring?: boolean;
  recurringFrequency?: string;
  recurringEndDate?: Date;
}

export interface UpdateTransactionDto {
  accountId?: string;
  categoryId?: string;
  amount?: number;
  description?: string;
  merchantName?: string;
  transactionDate?: Date;
  transactionType?: TransactionType;
  notes?: string;
  tags?: string[];
  referenceNumber?: string;
  isRecurring?: boolean;
  recurringFrequency?: string;
  recurringEndDate?: Date;
  isReconciled?: boolean;
}

export interface CreateCategoryDto {
  name: string;
  description?: string;
  categoryType: CategoryType;
  icon?: string;
  color?: string;
  parentCategoryId?: string;
  defaultVatRate: number;
  keywords?: string;
  isActive?: boolean;
}

export interface UpdateCategoryDto {
  name?: string;
  description?: string;
  icon?: string;
  color?: string;
  parentCategoryId?: string;
  defaultVatRate?: number;
  keywords?: string;
  isActive?: boolean;
}

export interface SuggestCategoryDto {
  description: string;
  merchantName?: string;
  amount?: number;
  transactionType: TransactionType;
}

export interface BulkUpdateTransactionsDto {
  transactionIds: string[];
  updates: UpdateTransactionDto;
}

export interface BulkUpdateCategoriesDto {
  categoryIds: string[];
  updates: UpdateCategoryDto;
}

// Query Parameters
export interface TransactionQueryParameters {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'ASC' | 'DESC';
  searchTerm?: string;
  categoryId?: string;
  accountId?: string;
  transactionType?: TransactionType;
  startDate?: Date;
  endDate?: Date;
  minAmount?: number;
  maxAmount?: number;
  isReconciled?: boolean;
  tags?: string[];
}

export interface CategoryQueryParameters {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'ASC' | 'DESC';
  searchTerm?: string;
  categoryType?: CategoryType;
  isActive?: boolean;
  includeSystemCategories?: boolean;
  parentCategoryId?: string;
}

// Specialized responses
export interface CategoryHierarchy {
  categoryType: CategoryType;
  categoryTypeName: string;
  categories: Category[];
}

export interface CategoryUsageStats {
  categoryId: string;
  categoryName: string;
  transactionCount: number;
  totalAmount: number;
  averageAmount: number;
  minAmount: number;
  maxAmount: number;
  firstTransactionDate: Date;
  lastTransactionDate: Date;
  monthlyUsage: MonthlyUsage[];
  subCategoryStats: CategoryUsageStats[];
}

export interface MonthlyUsage {
  year: number;
  month: number;
  transactionCount: number;
  totalAmount: number;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
