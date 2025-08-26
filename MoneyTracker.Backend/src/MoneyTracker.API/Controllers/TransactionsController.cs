// MoneyTracker.API/Controllers/TransactionsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MoneyTracker.API.Models;
using MoneyTracker.Application.Common.Exceptions;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.Transaction;
using System.Security.Claims;

namespace MoneyTracker.API.Controllers;

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
    /// Get transactions with filtering and pagination
    /// </summary>
    /// <param name="parameters">Query parameters for filtering</param>
    /// <returns>Paginated list of transactions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<TransactionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionQueryParameters parameters)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized(ApiResponse<object>.ErrorResult("Benutzer nicht authentifiziert", null, 401));

            if (!parameters.IsValid())
                return BadRequest(ApiResponse<object>.ErrorResult("Ungültige Abfrageparameter"));

            var result = await _transactionService.GetTransactionsAsync(Guid.Parse(userId), parameters);

            var pagination = new PaginationMetadata
            {
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                TotalCount = result.TotalCount,
                HasNext = result.HasNext,
                HasPrevious = result.HasPrevious
            };

            return Ok(PaginatedResponse<TransactionDto>.SuccessResult(
                result.Items,
                pagination,
                $"{result.TotalCount} Transaktionen gefunden"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for user {UserId}", _currentUserService.UserId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Interner Serverfehler", null, 500));
        }
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <returns>Transaction details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized(ApiResponse<object>.ErrorResult("Benutzer nicht authentifiziert", null, 401));

            var transaction = await _transactionService.GetTransactionByIdAsync(Guid.Parse(userId), id);
            if (transaction == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Transaktion mit ID {id} nicht gefunden", null, 404));

            return Ok(ApiResponse<TransactionDto>.SuccessResult(transaction, "Transaktion erfolgreich abgerufen"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId} for user {UserId}", id, _currentUserService.UserId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Interner Serverfehler", null, 500));
        }
    }

    /// <summary>
    /// Create a new transaction
    /// </summary>
    /// <param name="dto">Transaction creation data</param>
    /// <returns>Created transaction</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors)
                    .Select(e => new ValidationError
                    {
                        PropertyName = e.Exception?.Data["PropertyName"]?.ToString() ?? "Unknown",
                        ErrorMessage = e.ErrorMessage
                    })
                    .ToList();

                return BadRequest(ApiResponse<object>.ErrorResult("Validierung fehlgeschlagen", errors));
            }

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized(ApiResponse<object>.ErrorResult("Benutzer nicht authentifiziert", null, 401));

            var result = await _transactionService.CreateTransactionAsync(Guid.Parse(userId), dto);

            return CreatedAtAction(
                nameof(GetTransaction),
                new { id = result.Id },
                ApiResponse<TransactionDto>.SuccessResult(result, "Transaktion erfolgreich erstellt"));
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new ValidationError
            {
                PropertyName = e.Key,
                //ErrorMessage = e.ErrorMessage,
                AttemptedValue = e.Value
            }).ToList();

            return BadRequest(ApiResponse<object>.ErrorResult("Validierung fehlgeschlagen", errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction for user {UserId}", _currentUserService.UserId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Interner Serverfehler", null, 500));
        }
    }

    /// <summary>
    /// Update an existing transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="dto">Transaction update data</param>
    /// <returns>Updated transaction</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] UpdateTransactionDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<object>.ErrorResult("ID-Nichtübereinstimmung in URL und Body"));

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors)
                    .Select(e => new ValidationError
                    {
                        PropertyName = e.Exception?.Data["PropertyName"]?.ToString() ?? "Unknown",
                        ErrorMessage = e.ErrorMessage
                    })
                    .ToList();

                return BadRequest(ApiResponse<object>.ErrorResult("Validierung fehlgeschlagen", errors));
            }

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized(ApiResponse<object>.ErrorResult("Benutzer nicht authentifiziert", null, 401));

            var result = await _transactionService.UpdateTransactionAsync(Guid.Parse(userId), dto);
            if (result == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Transaktion mit ID {id} nicht gefunden", null, 404));

            return Ok(ApiResponse<TransactionDto>.SuccessResult(result, "Transaktion erfolgreich aktualisiert"));
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new ValidationError
            {
                PropertyName = e.Key,
                //ErrorMessage = e.ErrorMessage,
                AttemptedValue = e.Value
            }).ToList();

            return BadRequest(ApiResponse<object>.ErrorResult("Validierung fehlgeschlagen", errors));
        }
        catch (NotFoundException)
        {
            return NotFound(ApiResponse<object>.ErrorResult($"Transaktion mit ID {id} nicht gefunden", null, 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction {TransactionId} for user {UserId}", id, _currentUserService.UserId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Interner Serverfehler", null, 500));
        }
    }

    /// <summary>
    /// Delete a transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <returns>Success confirmation</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized(ApiResponse<object>.ErrorResult("Benutzer nicht authentifiziert", null, 401));

            var success = await _transactionService.DeleteTransactionAsync(Guid.Parse(userId), id);
            if (!success)
                return NotFound(ApiResponse<object>.ErrorResult($"Transaktion mit ID {id} nicht gefunden", null, 404));

            return Ok(ApiResponse<object>.SuccessResult(null, "Transaktion erfolgreich gelöscht"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {TransactionId} for user {UserId}", id, _currentUserService.UserId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Interner Serverfehler", null, 500));
        }
    }

    /// <summary>
    /// Get transaction summary statistics
    /// </summary>
    /// <param name="fromDate">Start date (optional)</param>
    /// <param name="toDate">End date (optional)</param>
    /// <returns>Transaction summary</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<TransactionSummaryDto>), 200)]
    public async Task<IActionResult> GetTransactionSummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized(ApiResponse<object>.ErrorResult("Benutzer nicht authentifiziert", null, 401));

            var summary = await _transactionService.GetTransactionSummaryAsync(Guid.Parse(userId), fromDate, toDate);
            return Ok(ApiResponse<TransactionSummaryDto>.SuccessResult(summary, "Zusammenfassung erfolgreich abgerufen"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction summary for user {UserId}", _currentUserService.UserId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Interner Serverfehler", null, 500));
        }
    }

    /// <summary>
    /// Bulk delete transactions
    /// </summary>
    /// <param name="ids">List of transaction IDs to delete</param>
    /// <returns>Bulk operation result</returns>
    [HttpPost("bulk-delete")]
    [ProducesResponseType(typeof(ApiResponse<BulkOperationResultDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> BulkDeleteTransactions([FromBody] List<Guid> ids)
    {
        try
        {
            if (ids == null || !ids.Any())
                return BadRequest(ApiResponse<object>.ErrorResult("Keine IDs zur Löschung angegeben"));

            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || (Guid.TryParse(userId, out Guid uid) && uid != Guid.Empty))
                return Unauthorized(ApiResponse<object>.ErrorResult("Benutzer nicht authentifiziert", null, 401));

            var result = await _transactionService.BulkDeleteTransactionsAsync(Guid.Parse(userId), ids);
            return Ok(ApiResponse<BulkOperationResultDto>.SuccessResult(result,
                $"{result.SuccessCount} von {result.TotalCount} Transaktionen erfolgreich gelöscht"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk delete transactions for user {UserId}", _currentUserService.UserId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Interner Serverfehler", null, 500));
        }
    }
}