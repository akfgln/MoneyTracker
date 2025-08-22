using MoneyTracker.Application.DTOs.File;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Interfaces;

public interface IPdfTextExtractionService
{
    Task<string> ExtractTextAsync(byte[] pdfBytes);
    Task<bool> ValidatePdfAsync(byte[] pdfBytes);
    Task<PdfMetadata> GetPdfMetadataAsync(byte[] pdfBytes);
    Task<bool> IsPdfEncryptedAsync(byte[] pdfBytes);
    Task<int> GetPageCountAsync(byte[] pdfBytes);
}

public interface IBankStatementParser
{
    Task<List<ExtractedTransactionDto>> ParseBankStatementAsync(string text, string bankName);
    bool SupportsBankFormat(string bankName);
    Task<BankStatementInfo> ExtractStatementInfoAsync(string text, string bankName);
    List<string> GetSupportedBanks();
}

public interface IBankFormatParser
{
    Task<List<ExtractedTransactionDto>> ParseTransactionsAsync(string text);
    Task<BankStatementInfo> ExtractStatementInfoAsync(string text);
    string BankName { get; }
    List<string> SupportedFormats { get; }
}

public interface IFileStorageService
{
    Task<string> StoreFileAsync(byte[] fileContent, string fileName, FileType fileType, Guid userId);
    Task<byte[]> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    Task<string> GenerateSecureFileName(string originalFileName, Guid userId);
    Task PerformVirusScanAsync(byte[] fileContent);
    Task<bool> FileExistsAsync(string filePath);
    Task<long> GetFileSizeAsync(string filePath);
    string GetContentType(string fileName);
    void EnsureDirectoryExists(string path);
}

public interface IDuplicateDetectionService
{
    Task<List<Transaction>> FindDuplicateTransactionsAsync(Guid userId, ExtractedTransactionDto extractedTransaction);
    Task<bool> IsDuplicateTransactionAsync(Guid userId, ExtractedTransactionDto extractedTransaction);
    Task<List<ExtractedTransactionDto>> MarkDuplicatesAsync(Guid userId, List<ExtractedTransactionDto> extractedTransactions);
    Task<double> CalculateSimilarityScoreAsync(Transaction existingTransaction, ExtractedTransactionDto extractedTransaction);
}

public interface IUploadedFileRepository
{
    Task<UploadedFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<UploadedFile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UploadedFile>> GetReceiptsByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<UploadedFile>> GetStatementsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UploadedFile?> GetByUserIdAndFileIdAsync(Guid userId, Guid fileId, CancellationToken cancellationToken = default);
    Task AddAsync(UploadedFile file, CancellationToken cancellationToken = default);
    void Update(UploadedFile file);
    void Delete(UploadedFile file);
    Task<int> GetReceiptCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UploadedFile>> GetFilesByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<bool> HasDuplicateFileAsync(Guid userId, string fileName, long fileSize, CancellationToken cancellationToken = default);
}