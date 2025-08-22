using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.Common.Models;
using MoneyTracker.Application.DTOs.Transaction;
using System.Security.Claims;

namespace MoneyTracker.API.Controllers;

/// <summary>
/// Transaction management controller with German VAT calculations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionService transactionService,
        ICurrentUserService currentUserService,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated transactions with filtering and sorting
    /// </summary>
    /// <param name="parameters">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of transactions</returns>
    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var result = await _transactionService.GetTransactionsAsync(userId, parameters, cancellationToken);
            
            return Ok(new
            {
                data = result.Items,
                pagination = new
                {
                    currentPage = result.Page,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    totalCount = result.TotalCount,
                    hasNextPage = result.HasNextPage,
                    hasPreviousPage = result.HasPreviousPage
                }
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for GetTransactions");
            return BadRequest(new { message = "Ungültige Parameter.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Get a specific transaction by ID
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var transaction = await _transactionService.GetTransactionByIdAsync(userId, id, cancellationToken);
            
            if (transaction == null)
            {
                return NotFound(new { message = "Transaktion nicht gefunden." });
            }

            return Ok(new { data = transaction });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {TransactionId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Create a new transaction with German VAT calculation
    /// </summary>
    /// <param name="dto">Transaction creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transaction</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var transaction = await _transactionService.CreateTransactionAsync(userId, dto, cancellationToken);
            
            return CreatedAtAction(
                nameof(GetTransaction),
                new { id = transaction.Id },
                new { data = transaction, message = "Transaktion erfolgreich erstellt." }
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data for CreateTransaction");
            return BadRequest(new { message = "Ungültige Transaktionsdaten.", details = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt in CreateTransaction");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Update an existing transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="dto">Transaction update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated transaction</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] UpdateTransactionDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var transaction = await _transactionService.UpdateTransactionAsync(userId, id, dto, cancellationToken);
            
            if (transaction == null)
            {
                return NotFound(new { message = "Transaktion nicht gefunden." });
            }

            return Ok(new { data = transaction, message = "Transaktion erfolgreich aktualisiert." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data for UpdateTransaction");
            return BadRequest(new { message = "Ungültige Transaktionsdaten.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction {TransactionId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Delete a transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var deleted = await _transactionService.DeleteTransactionAsync(userId, id, cancellationToken);
            
            if (!deleted)
            {
                return NotFound(new { message = "Transaktion nicht gefunden." });
            }

            return Ok(new { message = "Transaktion erfolgreich gelöscht." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {TransactionId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Search transactions with German text support
    /// </summary>
    /// <param name="searchDto">Search parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching transactions</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchTransactions([FromQuery] TransactionSearchDto searchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var transactions = await _transactionService.SearchTransactionsAsync(userId, searchDto, cancellationToken);
            
            return Ok(new 
            { 
                data = transactions,
                count = transactions.Count,
                searchQuery = searchDto.Query
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid search parameters");
            return BadRequest(new { message = "Ungültige Suchparameter.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching transactions for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Update transaction category
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="dto">Category update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPut("{id}/category")]
    public async Task<IActionResult> UpdateTransactionCategory(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var updated = await _transactionService.UpdateTransactionCategoryAsync(userId, id, dto.CategoryId, cancellationToken);
            
            if (!updated)
            {
                return NotFound(new { message = "Transaktion nicht gefunden." });
            }

            return Ok(new { message = "Kategorie erfolgreich aktualisiert." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid category update data");
            return BadRequest(new { message = "Ungültige Kategoriedaten.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category for transaction {TransactionId}", id);
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Bulk update multiple transactions
    /// </summary>
    /// <param name="dto">Bulk update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("bulk-update")]
    public async Task<IActionResult> BulkUpdateTransactions([FromBody] BulkUpdateTransactionsDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            if (dto.TransactionIds == null || !dto.TransactionIds.Any())
            {
                return BadRequest(new { message = "Keine Transaktionen zum Aktualisieren angegeben." });
            }

            var updated = await _transactionService.BulkUpdateTransactionsAsync(userId, dto, cancellationToken);
            
            if (!updated)
            {
                return BadRequest(new { message = "Keine gültigen Transaktionen gefunden." });
            }

            return Ok(new { message = $"{dto.TransactionIds.Count} Transaktionen erfolgreich aktualisiert." });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid bulk update data");
            return BadRequest(new { message = "Ungültige Bulk-Update-Daten.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating transactions for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Get transaction summary with VAT breakdown
    /// </summary>
    /// <param name="parameters">Summary parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction summary with German VAT details</returns>
    [HttpGet("summary")]
    public async Task<IActionResult> GetTransactionSummary([FromQuery] SummaryQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var summary = await _transactionService.GetTransactionSummaryAsync(userId, parameters, cancellationToken);
            
            return Ok(new { data = summary });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid summary parameters");
            return BadRequest(new { message = "Ungültige Zusammenfassungsparameter.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction summary for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Duplicate an existing transaction
    /// </summary>
    /// <param name="id">Transaction ID to duplicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Duplicated transaction</returns>
    [HttpPost("{id}/duplicate")]
    public async Task<IActionResult> DuplicateTransaction(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var duplicatedTransaction = await _transactionService.DuplicateTransactionAsync(userId, id, cancellationToken);
            
            if (duplicatedTransaction == null)
            {
                return NotFound(new { message = "Transaktion nicht gefunden." });
            }

            return CreatedAtAction(
                nameof(GetTransaction),
                new { id = duplicatedTransaction.Id },
                new { data = duplicatedTransaction, message = "Transaktion erfolgreich dupliziert." }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating transaction {TransactionId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Get recent transactions for dashboard
    /// </summary>
    /// <param name="count">Number of transactions to return (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent transactions</returns>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentTransactions([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            if (count <= 0 || count > 100)
            {
                return BadRequest(new { message = "Anzahl muss zwischen 1 und 100 liegen." });
            }

            var transactions = await _transactionService.GetRecentTransactionsAsync(userId, count, cancellationToken);
            
            return Ok(new { data = transactions, count = transactions.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent transactions for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Verify a transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/verify")]
    public async Task<IActionResult> VerifyTransaction(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var verifiedBy = User.Identity?.Name ?? "System";
            var verified = await _transactionService.VerifyTransactionAsync(userId, id, verifiedBy, cancellationToken);
            
            if (!verified)
            {
                return NotFound(new { message = "Transaktion nicht gefunden." });
            }

            return Ok(new { message = "Transaktion erfolgreich verifiziert." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying transaction {TransactionId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Process a pending transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/process")]
    public async Task<IActionResult> ProcessPendingTransaction(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Benutzer nicht authentifiziert." });
            }

            var processed = await _transactionService.ProcessPendingTransactionAsync(userId, id, cancellationToken);
            
            if (!processed)
            {
                return NotFound(new { message = "Transaktion nicht gefunden." });
            }

            return Ok(new { message = "Transaktion erfolgreich verarbeitet." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transaction {TransactionId} for user {UserId}", id, GetCurrentUserId());
            return StatusCode(500, new { message = "Ein interner Serverfehler ist aufgetreten." });
        }
    }

    /// <summary>
    /// Get current user ID from JWT token
    /// </summary>
    /// <returns>User ID or Guid.Empty if not found</returns>
    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value 
                          ?? User.FindFirst("userId")?.Value;
                          
        if (string.IsNullOrEmpty(userIdString))
        {
            return _currentUserService.UserId ?? Guid.Empty;
        }
        
        return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
    }
}