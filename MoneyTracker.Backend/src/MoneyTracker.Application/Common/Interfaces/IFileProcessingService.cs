using MoneyTracker.Application.DTOs.File;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Application.Common.Interfaces;

/// <summary>
/// Service for processing uploaded files (PDF extraction, parsing, etc.)
/// </summary>
public interface IFileProcessingService
{
    /// <summary>
    /// Process an uploaded file asynchronously (text extraction, parsing, etc.)
    /// </summary>
    /// <param name="fileId">The ID of the uploaded file to process</param>
    /// <returns>Task representing the async operation</returns>
    Task ProcessUploadedFileAsync(Guid fileId);

    /// <summary>
    /// Get processing status for a file
    /// </summary>
    /// <param name="fileId">The ID of the uploaded file</param>
    /// <returns>File processing status and results</returns>
    Task<FileProcessingResultDto> GetProcessingStatusAsync(Guid fileId);

    /// <summary>
    /// Extract text from PDF file
    /// </summary>
    /// <param name="fileId">The ID of the uploaded PDF file</param>
    /// <returns>Extracted text content</returns>
    Task<string> ExtractTextFromFileAsync(Guid fileId);

    /// <summary>
    /// Parse bank statement and extract transaction data
    /// </summary>
    /// <param name="fileId">The ID of the uploaded bank statement</param>
    /// <returns>List of extracted transactions</returns>
    Task<List<ExtractedTransactionDto>> ParseBankStatementAsync(Guid fileId);

    /// <summary>
    /// Validate file format and content
    /// </summary>
    /// <param name="fileId">The ID of the uploaded file</param>
    /// <returns>Validation result</returns>
    Task<FileValidationResultDto> ValidateFileAsync(Guid fileId);

    /// <summary>
    /// Validate file content before upload
    /// </summary>
    /// <param name="fileContent">The file content bytes</param>
    /// <param name="fileName">The file name</param>
    /// <param name="fileType">The expected file type</param>
    /// <returns>True if valid, false otherwise</returns>
    Task<bool> ValidateFileAsync(byte[] fileContent, string fileName, string fileType);

    /// <summary>
    /// Generate preview of bank statement with extracted transactions
    /// </summary>
    /// <param name="fileId">The ID of the uploaded file</param>
    /// <returns>Bank statement preview with transactions</returns>
    Task<BankStatementPreviewDto> GeneratePreviewAsync(Guid fileId);

    /// <summary>
    /// Import selected transactions from bank statement preview
    /// </summary>
    /// <param name="fileId">The ID of the uploaded file</param>
    /// <param name="importRequest">Import configuration</param>
    /// <returns>Import result</returns>
    Task<ImportResultDto> ImportTransactionsAsync(Guid fileId, ImportTransactionsDto importRequest);
}
