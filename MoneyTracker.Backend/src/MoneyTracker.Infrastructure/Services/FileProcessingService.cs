using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.File;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Services;

/// <summary>
/// Service for processing uploaded files (PDF extraction, parsing, etc.)
/// </summary>
public class FileProcessingService : IFileProcessingService
{
    private readonly IUploadedFileRepository _fileRepository;
    private readonly IPdfTextExtractionService _pdfExtraction;
    private readonly IBankStatementParser _bankStatementParser;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FileProcessingService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public FileProcessingService(
        IUploadedFileRepository fileRepository,
        IPdfTextExtractionService pdfExtraction,
        IBankStatementParser bankStatementParser,
        IFileStorageService fileStorage,
        ILogger<FileProcessingService> logger,
        IUnitOfWork unitOfWork)
    {
        _fileRepository = fileRepository;
        _pdfExtraction = pdfExtraction;
        _bankStatementParser = bankStatementParser;
        _fileStorage = fileStorage;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessUploadedFileAsync(Guid fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            _logger.LogWarning("File with ID {FileId} not found for processing", fileId);
            return;
        }

        try
        {
            _logger.LogInformation("Starting processing for file {FileId} ({FileName})", fileId, file.OriginalFileName);
            
            // Mark as processing
            file.MarkAsProcessing("Verarbeitung gestartet...");
            await _unitOfWork.SaveChangesAsync();

            // Extract text from PDF
            var extractedText = await ExtractTextFromFileAsync(fileId);
            
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                file.MarkAsFailed("Kein Text konnte aus der PDF extrahiert werden.");
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            // If it's a bank statement, parse transactions
            if (file.FileType == FileType.BankStatement)
            {
                var transactions = await ParseBankStatementAsync(fileId);
                file.ExtractedDataJson = System.Text.Json.JsonSerializer.Serialize(transactions);
                file.MarkAsProcessed(extractedText, file.ExtractedDataJson);
                
                _logger.LogInformation("Successfully processed bank statement {FileId} with {TransactionCount} transactions", 
                    fileId, transactions.Count);
            }
            else
            {
                // For receipts, just mark as processed with extracted text
                file.MarkAsProcessed(extractedText);
                _logger.LogInformation("Successfully processed receipt {FileId}", fileId);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process file {FileId}", fileId);
            file.MarkAsFailed($"Verarbeitungsfehler: {ex.Message}");
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<FileProcessingResultDto> GetProcessingStatusAsync(Guid fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            throw new ArgumentException($"File with ID {fileId} not found", nameof(fileId));
        }

        var result = new FileProcessingResultDto
        {
            FileId = file.Id,
            Status = file.Status,
            ProcessingMessage = file.ProcessingMessage,
            ProcessedDate = file.ProcessedDate,
            ExtractedText = file.ExtractedText
        };

        // If bank statement with extracted data, deserialize transactions
        if (file.FileType == FileType.BankStatement && !string.IsNullOrEmpty(file.ExtractedDataJson))
        {
            try
            {
                result.ExtractedTransactions = System.Text.Json.JsonSerializer
                    .Deserialize<List<ExtractedTransactionDto>>(file.ExtractedDataJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize extracted transaction data for file {FileId}", fileId);
            }
        }

        return result;
    }

    public async Task<string> ExtractTextFromFileAsync(Guid fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            throw new ArgumentException($"File with ID {fileId} not found", nameof(fileId));
        }

        // Get file content from storage
        var fileContent = await _fileStorage.GetFileAsync(file.FilePath);
        
        // Extract text using PDF service
        var extractedText = await _pdfExtraction.ExtractTextAsync(fileContent);
        
        // Update file record with extracted text
        file.ExtractedText = extractedText;
        await _unitOfWork.SaveChangesAsync();

        return extractedText;
    }

    public async Task<List<ExtractedTransactionDto>> ParseBankStatementAsync(Guid fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            throw new ArgumentException($"File with ID {fileId} not found", nameof(fileId));
        }

        if (file.FileType != FileType.BankStatement)
        {
            throw new InvalidOperationException("File is not a bank statement");
        }

        // Ensure we have extracted text
        var extractedText = file.ExtractedText;
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            extractedText = await ExtractTextFromFileAsync(fileId);
        }

        // Parse using bank statement parser
        var transactions = await _bankStatementParser.ParseBankStatementAsync(
            extractedText, 
            file.BankName ?? "unknown"
        );

        _logger.LogInformation("Parsed {TransactionCount} transactions from bank statement {FileId}", 
            transactions.Count, fileId);

        return transactions;
    }

    public async Task<FileValidationResultDto> ValidateFileAsync(Guid fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file == null)
        {
            return new FileValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "File not found"
            };
        }

        var result = new FileValidationResultDto
        {
            ValidationErrors = new List<string>()
        };

        try
        {
            // Get file content
            var fileContent = await _fileStorage.GetFileAsync(file.FilePath);
            
            // Validate PDF
            var isValidPdf = await _pdfExtraction.ValidatePdfAsync(fileContent);
            if (!isValidPdf)
            {
                result.ValidationErrors.Add("Invalid or corrupted PDF file");
            }

            // Get PDF metadata
            var metadata = await _pdfExtraction.GetPdfMetadataAsync(fileContent);
            result.FileMetadata = new FileMetadata
            {
                NumberOfPages = metadata.NumberOfPages,
                Title = metadata.Title,
                Author = metadata.Author,
                CreationDate = metadata.CreationDate,
                Creator = metadata.Creator,
                FileSizeBytes = file.FileSize,
                ContentType = file.ContentType
            };

            // Additional validations
            if (file.FileSize == 0)
            {
                result.ValidationErrors.Add("File is empty");
            }

            if (file.FileSize > 10 * 1024 * 1024) // 10MB limit
            {
                result.ValidationErrors.Add("File size exceeds 10MB limit");
            }

            result.IsValid = result.ValidationErrors.Count == 0;
            result.ErrorMessage = result.IsValid ? null : string.Join("; ", result.ValidationErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate file {FileId}", fileId);
            result.IsValid = false;
            result.ErrorMessage = "File validation failed: " + ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Validate file content before upload (placeholder implementation)
    /// </summary>
    public async Task<bool> ValidateFileAsync(byte[] fileContent, string fileName, string fileType)
    {
        try
        {
            if (fileContent == null || fileContent.Length == 0)
                return false;

            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            // Basic PDF validation for file content
            if (fileType?.ToLower() == "pdf" || fileName.ToLower().EndsWith(".pdf"))
            {
                return await _pdfExtraction.ValidatePdfAsync(fileContent);
            }

            return true; // For other file types, basic validation passes
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File validation failed for {FileName}", fileName);
            return false;
        }
    }

    /// <summary>
    /// Generate preview of bank statement (placeholder implementation)
    /// </summary>
    public async Task<BankStatementPreviewDto> GeneratePreviewAsync(Guid fileId)
    {
        try
        {
            var file = await _fileRepository.GetByIdAsync(fileId);
            if (file == null)
            {
                throw new FileNotFoundException($"File with ID {fileId} not found");
            }

            // Parse bank statement to extract transactions
            var transactions = await ParseBankStatementAsync(fileId);

            return new BankStatementPreviewDto
            {
                FileId = fileId,
                FileName = file.OriginalFileName,
                BankName = "Unknown", // Would be extracted from parsing
                Transactions = transactions,
                TotalTransactions = transactions.Count,
                NewTransactions = transactions.Count(t => !t.IsDuplicate),
                DuplicateTransactions = transactions.Count(t => t.IsDuplicate),
                TotalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount),
                TotalExpenses = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount)),
                ProcessingStatus = "Completed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate preview for file {FileId}", fileId);
            throw;
        }
    }

    /// <summary>
    /// Import selected transactions (placeholder implementation)
    /// </summary>
    public async Task<ImportResultDto> ImportTransactionsAsync(Guid fileId, ImportTransactionsDto importRequest)
    {
        try
        {
            // This would contain the actual import logic
            _logger.LogInformation("Importing {Count} transactions from file {FileId}", 
                importRequest.SelectedTransactionIds.Count, fileId);

            return new ImportResultDto
            {
                TotalTransactions = importRequest.SelectedTransactionIds.Count,
                ImportedTransactions = importRequest.SelectedTransactionIds.Count,
                SkippedDuplicates = 0,
                FailedTransactions = 0,
                ImportDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import transactions from file {FileId}", fileId);
            throw;
        }
    }
}
