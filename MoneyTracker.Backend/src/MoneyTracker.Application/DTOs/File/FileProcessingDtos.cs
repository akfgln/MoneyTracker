using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.File;

/// <summary>
/// Result of file processing operation
/// </summary>
public class FileProcessingResultDto
{
    public Guid FileId { get; set; }
    public FileStatus Status { get; set; }
    public string? ProcessingMessage { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public bool IsSuccessful => Status == FileStatus.Processed || Status == FileStatus.Imported;
    public bool HasFailed => Status == FileStatus.Failed || Status == FileStatus.VirusDetected;
    public string? ExtractedText { get; set; }
    public List<ExtractedTransactionDto>? ExtractedTransactions { get; set; }
    public int TransactionCount => ExtractedTransactions?.Count ?? 0;
}

/// <summary>
/// Result of file validation operation
/// </summary>
public class FileValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public FileMetadata? FileMetadata { get; set; }
}

/// <summary>
/// Metadata about an uploaded file
/// </summary>
public class FileMetadata
{
    public int NumberOfPages { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public DateTime? CreationDate { get; set; }
    public string? Creator { get; set; }
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
}
