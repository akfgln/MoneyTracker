using Microsoft.AspNetCore.Http;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.File;

public class UploadReceiptDto
{
    public IFormFile File { get; set; } = null!;
    public Guid? TransactionId { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
}

public class UploadStatementDto
{
    public IFormFile File { get; set; } = null!;
    public Guid AccountId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public DateTime? StatementPeriodStart { get; set; }
    public DateTime? StatementPeriodEnd { get; set; }
}

public class ImportTransactionsDto
{
    public List<string> SelectedTransactionIds { get; set; } = new();
    public bool SkipDuplicates { get; set; } = true;
    public Guid? DefaultCategoryId { get; set; }
    public bool AutoCategorize { get; set; } = true;
}

public class LinkTransactionDto
{
    public Guid TransactionId { get; set; }
    public string? Description { get; set; }
}

public class UploadFileResponseDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public FileStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string? ProcessingMessage { get; set; }
    public FileType FileType { get; set; }
    public string FileTypeDisplay { get; set; } = string.Empty;
    public Guid? TransactionId { get; set; }
    public Guid? AccountId { get; set; }
    public string? BankName { get; set; }
    public DateTime? StatementPeriodStart { get; set; }
    public DateTime? StatementPeriodEnd { get; set; }
    public string? Tags { get; set; }
    public bool CanDownload { get; set; }
    public bool CanDelete { get; set; }
    public bool CanProcess { get; set; }
    public string? VirusScanResult { get; set; }
    public DateTime? VirusScanDate { get; set; }
}

public class ExtractedTransactionDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime? BookingDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? MerchantName { get; set; }
    public string? ReferenceNumber { get; set; }
    public TransactionType TransactionType { get; set; }
    public string TransactionTypeDisplay { get; set; } = string.Empty;
    public Guid? SuggestedCategoryId { get; set; }
    public string? SuggestedCategoryName { get; set; }
    public bool IsDuplicate { get; set; }
    public Guid? DuplicateTransactionId { get; set; }
    public string? DuplicateReason { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Location { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsSelected { get; set; } = true;
    public string? Notes { get; set; }
}

public class BankStatementPreviewDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public DateTime? StatementPeriodStart { get; set; }
    public DateTime? StatementPeriodEnd { get; set; }
    public List<ExtractedTransactionDto> Transactions { get; set; } = new();
    public int TotalTransactions { get; set; }
    public int DuplicateTransactions { get; set; }
    public int NewTransactions { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetAmount { get; set; }
    public string ProcessingStatus { get; set; } = string.Empty;
    public List<string> ProcessingWarnings { get; set; } = new();
    public List<string> ProcessingErrors { get; set; } = new();
}

public class ImportResultDto
{
    public int TotalTransactions { get; set; }
    public int ImportedTransactions { get; set; }
    public int SkippedDuplicates { get; set; }
    public int FailedTransactions { get; set; }
    public List<string> SuccessfulTransactionIds { get; set; } = new();
    public List<string> FailedTransactionIds { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public decimal TotalImportedAmount { get; set; }
    public DateTime ImportDate { get; set; }
}

public class PdfMetadata
{
    public int NumberOfPages { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public DateTime? CreationDate { get; set; }
    public string? Creator { get; set; }
    public string? Subject { get; set; }
    public string? Keywords { get; set; }
    public long FileSize { get; set; }
    public string PdfVersion { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public bool HasDigitalSignature { get; set; }
}

public class BankStatementInfo
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public DateTime? StatementPeriodStart { get; set; }
    public DateTime? StatementPeriodEnd { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public string Currency { get; set; } = "EUR";
    public int TransactionCount { get; set; }
    public string StatementNumber { get; set; } = string.Empty;
    public DateTime? GenerationDate { get; set; }
}