using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.File;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.API.Controllers;

/// <summary>
/// File upload and processing controller for bank statements and receipts
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private readonly IFileProcessingService _fileProcessing;
    private readonly IUploadedFileRepository _fileRepository;
    private readonly IDuplicateDetectionService _duplicateDetection;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FilesController> _logger;

    private const int MaxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] AllowedFileTypes = { ".pdf" };

    public FilesController(
        IFileStorageService fileStorage,
        IFileProcessingService fileProcessing,
        IUploadedFileRepository fileRepository,
        IDuplicateDetectionService duplicateDetection,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<FilesController> logger)
    {
        _fileStorage = fileStorage;
        _fileProcessing = fileProcessing;
        _fileRepository = fileRepository;
        _duplicateDetection = duplicateDetection;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Upload a receipt file and link it to a transaction
    /// </summary>
    [HttpPost("upload-receipt")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadReceipt([FromForm] UploadReceiptDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            // Validate file
            var validationResult = await ValidateUploadedFile(dto.File, FileType.Receipt);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            // Read file content
            using var memoryStream = new MemoryStream();
            await dto.File.CopyToAsync(memoryStream);
            var fileContent = memoryStream.ToArray();

            // Store file
            var filePath = await _fileStorage.StoreFileAsync(fileContent, dto.File.FileName, FileType.Receipt, userId);

            // Create file record
            var uploadedFile = new UploadedFile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OriginalFileName = dto.File.FileName,
                StoredFileName = Path.GetFileName(filePath),
                FilePath = filePath,
                ContentType = dto.File.ContentType,
                FileSize = dto.File.Length,
                FileType = FileType.Receipt,
                Status = FileStatus.Uploaded,
                UploadDate = DateTime.UtcNow,
                TransactionId = dto.TransactionId,
                Tags = dto.Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _fileRepository.AddAsync(uploadedFile);
            await _unitOfWork.SaveChangesAsync();

            // Process file asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    await _fileProcessing.ProcessUploadedFileAsync(uploadedFile.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process uploaded receipt {FileId}", uploadedFile.Id);
                }
            });

            var response = MapToResponseDto(uploadedFile);
            _logger.LogInformation("Receipt uploaded successfully: {FileName} for user {UserId}", dto.File.FileName, userId);

            return Ok(new { data = response, message = "Beleg erfolgreich hochgeladen." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload receipt");
            return StatusCode(500, new { message = "Fehler beim Hochladen des Belegs." });
        }
    }

    /// <summary>
    /// Upload a bank statement PDF for processing
    /// </summary>
    [HttpPost("upload-statement")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadBankStatement([FromForm] UploadStatementDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            // Validate file
            var validationResult = await ValidateUploadedFile(dto.File, FileType.BankStatement);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            // Read file content
            using var memoryStream = new MemoryStream();
            await dto.File.CopyToAsync(memoryStream);
            var fileContent = memoryStream.ToArray();

            // Check for duplicate files
            var isDuplicate = await _fileRepository.HasDuplicateFileAsync(userId, dto.File.FileName, dto.File.Length);
            if (isDuplicate)
            {
                return BadRequest(new { message = "Eine identische Datei wurde bereits innerhalb der letzten Stunde hochgeladen." });
            }

            // Store file
            var filePath = await _fileStorage.StoreFileAsync(fileContent, dto.File.FileName, FileType.BankStatement, userId);

            // Create file record
            var uploadedFile = new UploadedFile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OriginalFileName = dto.File.FileName,
                StoredFileName = Path.GetFileName(filePath),
                FilePath = filePath,
                ContentType = dto.File.ContentType,
                FileSize = dto.File.Length,
                FileType = FileType.BankStatement,
                Status = FileStatus.Uploaded,
                UploadDate = DateTime.UtcNow,
                AccountId = dto.AccountId,
                BankName = dto.BankName,
                StatementPeriodStart = dto.StatementPeriodStart,
                StatementPeriodEnd = dto.StatementPeriodEnd,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _fileRepository.AddAsync(uploadedFile);
            await _unitOfWork.SaveChangesAsync();

            // Process file asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    await _fileProcessing.ProcessUploadedFileAsync(uploadedFile.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process uploaded bank statement {FileId}", uploadedFile.Id);
                }
            });

            var response = MapToResponseDto(uploadedFile);
            _logger.LogInformation("Bank statement uploaded successfully: {FileName} for user {UserId}", dto.File.FileName, userId);

            return Ok(new { data = response, message = "Kontoauszug erfolgreich hochgeladen und wird verarbeitet." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload bank statement");
            return StatusCode(500, new { message = "Fehler beim Hochladen des Kontoauszugs." });
        }
    }

    /// <summary>
    /// Download a file by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var file = await _fileRepository.GetByUserIdAndFileIdAsync(userId, id);

            if (file == null)
            {
                return NotFound(new { message = "Datei nicht gefunden." });
            }

            var fileContent = await _fileStorage.GetFileAsync(file.FilePath);

            return File(fileContent, file.ContentType, file.OriginalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file {FileId}", id);
            return StatusCode(500, new { message = "Fehler beim Herunterladen der Datei." });
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var file = await _fileRepository.GetByUserIdAndFileIdAsync(userId, id);

            if (file == null)
            {
                return NotFound(new { message = "Datei nicht gefunden." });
            }

            // Delete physical file
            await _fileStorage.DeleteFileAsync(file.FilePath);

            // Mark as deleted
            _fileRepository.Delete(file);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("File deleted: {FileId} by user {UserId}", id, userId);
            return Ok(new { message = "Datei erfolgreich gelöscht." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FileId}", id);
            return StatusCode(500, new { message = "Fehler beim Löschen der Datei." });
        }
    }

    /// <summary>
    /// Preview extracted data from a bank statement
    /// </summary>
    [HttpGet("statement/{id}/preview")]
    public async Task<IActionResult> PreviewExtractedData(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var file = await _fileRepository.GetByUserIdAndFileIdAsync(userId, id);

            if (file == null)
            {
                return NotFound(new { message = "Datei nicht gefunden." });
            }

            if (file.FileType != FileType.BankStatement)
            {
                return BadRequest(new { message = "Vorschau ist nur für Kontoauszüge verfügbar." });
            }

            if (!file.IsProcessed)
            {
                return BadRequest(new { message = "Datei wird noch verarbeitet. Bitte versuchen Sie es später erneut." });
            }

            var preview = await _fileProcessing.GeneratePreviewAsync(id);
            return Ok(new { data = preview });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate preview for file {FileId}", id);
            return StatusCode(500, new { message = "Fehler beim Erstellen der Vorschau." });
        }
    }

    /// <summary>
    /// Import selected transactions from a bank statement
    /// </summary>
    [HttpPost("statement/{id}/import")]
    public async Task<IActionResult> ImportTransactions(Guid id, [FromBody] ImportTransactionsDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var file = await _fileRepository.GetByUserIdAndFileIdAsync(userId, id);

            if (file == null)
            {
                return NotFound(new { message = "Datei nicht gefunden." });
            }

            if (file.FileType != FileType.BankStatement)
            {
                return BadRequest(new { message = "Import ist nur für Kontoauszüge verfügbar." });
            }

            if (!file.IsProcessed)
            {
                return BadRequest(new { message = "Datei muss erst verarbeitet werden." });
            }

            var result = await _fileProcessing.ImportTransactionsAsync(id, dto);

            _logger.LogInformation("Import completed for file {FileId}: {Imported}/{Total} transactions",
                id, result.ImportedTransactions, result.TotalTransactions);

            return Ok(new { data = result, message = $"{result.ImportedTransactions} Transaktionen erfolgreich importiert." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import transactions from file {FileId}", id);
            return StatusCode(500, new { message = "Fehler beim Importieren der Transaktionen." });
        }
    }

    /// <summary>
    /// Get user's receipts with pagination
    /// </summary>
    [HttpGet("receipts")]
    public async Task<IActionResult> GetUserReceipts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (pageSize > 100) pageSize = 100;
            if (page < 1) page = 1;

            var receipts = await _fileRepository.GetReceiptsByUserIdAsync(userId, page, pageSize);
            var totalCount = await _fileRepository.GetReceiptCountByUserIdAsync(userId);

            var response = receipts.Select(MapToResponseDto).ToList();

            return Ok(new
            {
                data = response,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user receipts");
            return StatusCode(500, new { message = "Fehler beim Laden der Belege." });
        }
    }

    /// <summary>
    /// Link a file to a transaction
    /// </summary>
    [HttpPost("{id}/link-transaction")]
    public async Task<IActionResult> LinkToTransaction(Guid id, [FromBody] LinkTransactionDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var file = await _fileRepository.GetByUserIdAndFileIdAsync(userId, id);

            if (file == null)
            {
                return NotFound(new { message = "Datei nicht gefunden." });
            }

            file.LinkToTransaction(dto.TransactionId);
            _fileRepository.Update(file);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Datei erfolgreich mit Transaktion verknüpft." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to link file {FileId} to transaction {TransactionId}", id, dto.TransactionId);
            return StatusCode(500, new { message = "Fehler beim Verknüpfen der Datei." });
        }
    }

    private async Task<FileValidationResult> ValidateUploadedFile(IFormFile file, FileType fileType)
    {
        // Check if file is provided
        if (file == null || file.Length == 0)
        {
            return new FileValidationResult(false, "Keine Datei ausgewählt.");
        }

        // Check file size
        if (file.Length > MaxFileSize)
        {
            return new FileValidationResult(false, $"Datei ist zu groß. Maximale Größe: {MaxFileSize / (1024 * 1024)} MB");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedFileTypes.Contains(extension))
        {
            return new FileValidationResult(false, $"Dateityp nicht unterstützt. Erlaubte Typen: {string.Join(", ", AllowedFileTypes)}");
        }

        // Read file content for validation
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileContent = memoryStream.ToArray();

        // Validate file content
        var isValid = await _fileProcessing.ValidateFileAsync(fileContent, file.FileName, fileType.ToString());
        if (!isValid)
        {
            return new FileValidationResult(false, "Datei ist beschädigt oder ungültig.");
        }

        return new FileValidationResult(true, null);
    }

    private UploadFileResponseDto MapToResponseDto(UploadedFile file)
    {
        return new UploadFileResponseDto
        {
            FileId = file.Id,
            FileName = file.OriginalFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            FileSizeFormatted = file.FileSizeFormatted,
            UploadDate = file.UploadDate,
            Status = file.Status,
            StatusDisplay = GetStatusDisplay(file.Status),
            ProcessingMessage = file.ProcessingMessage,
            FileType = file.FileType,
            FileTypeDisplay = GetFileTypeDisplay(file.FileType),
            TransactionId = file.TransactionId,
            AccountId = file.AccountId,
            BankName = file.BankName,
            StatementPeriodStart = file.StatementPeriodStart,
            StatementPeriodEnd = file.StatementPeriodEnd,
            Tags = file.Tags,
            CanDownload = file.Status == FileStatus.Processed || file.Status == FileStatus.Imported,
            CanDelete = true,
            CanProcess = file.Status == FileStatus.Uploaded || file.Status == FileStatus.Failed,
            VirusScanResult = file.VirusScanResult,
            VirusScanDate = file.VirusScanDate
        };
    }

    private string GetStatusDisplay(FileStatus status)
    {
        return status switch
        {
            FileStatus.Uploaded => "Hochgeladen",
            FileStatus.Processing => "Wird verarbeitet",
            FileStatus.Processed => "Verarbeitet",
            FileStatus.Failed => "Fehler",
            FileStatus.Imported => "Importiert",
            FileStatus.VirusDetected => "Virus erkannt",
            _ => status.ToString()
        };
    }

    private string GetFileTypeDisplay(FileType fileType)
    {
        return fileType switch
        {
            FileType.Receipt => "Beleg",
            FileType.BankStatement => "Kontoauszug",
            FileType.Document => "Dokument",
            FileType.Invoice => "Rechnung",
            FileType.Contract => "Vertrag",
            _ => fileType.ToString()
        };
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value
                          ?? User.FindFirst("userId")?.Value;
        Guid uid = Guid.Empty;
        if (string.IsNullOrEmpty(userIdString) || Guid.TryParse(_currentUserService.UserId, out uid))
        {
            return uid;
        }

        return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
    }

    private record FileValidationResult(bool IsValid, string? ErrorMessage);
}