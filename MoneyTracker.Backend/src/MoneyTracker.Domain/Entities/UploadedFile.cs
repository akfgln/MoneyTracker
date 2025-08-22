using MoneyTracker.Domain.Common;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Domain.Entities;

public class UploadedFile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public FileType FileType { get; set; }
    public FileStatus Status { get; set; }
    public string? ProcessingMessage { get; set; }
    public DateTime UploadDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public Guid? TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }
    public string? ExtractedText { get; set; }
    public string? ExtractedDataJson { get; set; }
    public string? Tags { get; set; }
    public bool IsDeleted { get; set; }
    public string? BankName { get; set; }
    public DateTime? StatementPeriodStart { get; set; }
    public DateTime? StatementPeriodEnd { get; set; }
    public string? VirusScanResult { get; set; }
    public DateTime? VirusScanDate { get; set; }

    // Computed properties
    public string DisplayName => !string.IsNullOrEmpty(OriginalFileName) ? OriginalFileName : StoredFileName;
    public bool IsProcessing => Status == FileStatus.Processing;
    public bool IsProcessed => Status == FileStatus.Processed || Status == FileStatus.Imported;
    public bool HasFailed => Status == FileStatus.Failed;
    public string FileSizeFormatted => FormatFileSize(FileSize);

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public void MarkAsProcessing(string? message = null)
    {
        Status = FileStatus.Processing;
        ProcessingMessage = message;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed(string? extractedText = null, string? extractedDataJson = null)
    {
        Status = FileStatus.Processed;
        ProcessedDate = DateTime.UtcNow;
        ExtractedText = extractedText;
        ExtractedDataJson = extractedDataJson;
        ProcessingMessage = "Erfolgreich verarbeitet";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = FileStatus.Failed;
        ProcessingMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsImported()
    {
        Status = FileStatus.Imported;
        ProcessingMessage = "Transaktionen erfolgreich importiert";
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToTransaction(Guid transactionId)
    {
        TransactionId = transactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetVirusScanResult(bool isClean, string result)
    {
        VirusScanResult = isClean ? "Sauber" : result;
        VirusScanDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}