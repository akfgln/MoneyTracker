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
}
