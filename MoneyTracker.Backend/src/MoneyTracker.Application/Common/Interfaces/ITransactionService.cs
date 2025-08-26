using MoneyTracker.Application.DTOs.Transaction;
using MoneyTracker.Application.Common.Models;

namespace MoneyTracker.Application.Common.Interfaces;

public interface ITransactionService
{
    Task<PagedResult<TransactionResponseDto>> GetTransactionsAsync(Guid userId, TransactionQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<TransactionResponseDto?> GetTransactionByIdAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);
    Task<TransactionResponseDto> CreateTransactionAsync(Guid userId, CreateTransactionDto dto, CancellationToken cancellationToken = default);
    Task<TransactionResponseDto?> UpdateTransactionAsync(Guid userId, Guid transactionId, UpdateTransactionDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);
    Task<List<TransactionResponseDto>> SearchTransactionsAsync(Guid userId, TransactionSearchDto searchDto, CancellationToken cancellationToken = default);
    Task<bool> BulkUpdateTransactionsAsync(Guid userId, BulkUpdateTransactionsDto dto, CancellationToken cancellationToken = default);
    Task<TransactionSummaryDto> GetTransactionSummaryAsync(Guid userId, SummaryQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<TransactionResponseDto?> DuplicateTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);
    Task<bool> UpdateTransactionCategoryAsync(Guid userId, Guid transactionId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<List<TransactionResponseDto>> GetRecentTransactionsAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default);
    Task<bool> VerifyTransactionAsync(Guid userId, Guid transactionId, string verifiedBy, CancellationToken cancellationToken = default);
    Task<bool> ProcessPendingTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);

    Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(Guid userId, TransactionQueryParameters parameters);
    Task<TransactionDto?> GetTransactionByIdAsync(Guid userId, Guid transactionId);
    Task<TransactionDto> CreateTransactionAsync(Guid userId, CreateTransactionDto dto);
    Task<TransactionDto?> UpdateTransactionAsync(Guid userId, UpdateTransactionDto dto);
    Task<bool> DeleteTransactionAsync(Guid userId, Guid transactionId);
    Task<TransactionSummaryDto> GetTransactionSummaryAsync(Guid userId, DateTime? fromDate, DateTime? toDate);
    Task<BulkOperationResultDto> BulkDeleteTransactionsAsync(Guid userId, List<Guid> transactionIds);
    Task<List<TransactionDto>> GetRecentTransactionsAsync(Guid userId, int count = 10);
}
