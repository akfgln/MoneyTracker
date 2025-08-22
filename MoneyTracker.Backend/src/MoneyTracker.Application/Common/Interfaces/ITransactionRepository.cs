using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    // User-specific queries
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetRecentByUserIdAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default);
    
    // Account-specific queries
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByAccountIdAndDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    // Category-specific queries
    Task<IEnumerable<Transaction>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByCategoryIdAndDateRangeAsync(Guid categoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    // Transaction type queries
    Task<IEnumerable<Transaction>> GetByTransactionTypeAsync(TransactionType transactionType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetIncomeByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetExpensesByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    // Search and filtering
    Task<IEnumerable<Transaction>> SearchTransactionsAsync(Guid userId, string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByMerchantNameAsync(string merchantName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByPaymentMethodAsync(string paymentMethod, CancellationToken cancellationToken = default);
    
    // Status-based queries
    Task<IEnumerable<Transaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetUnverifiedTransactionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync(CancellationToken cancellationToken = default);
    
    // Import-related queries
    Task<IEnumerable<Transaction>> GetByImportSourceAsync(string importSource, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByExternalTransactionIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<bool> ExternalTransactionExistsAsync(string externalId, CancellationToken cancellationToken = default);
    
    // Aggregation queries
    Task<decimal> GetTotalIncomeByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalExpensesByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetNetBalanceByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    Task<decimal> GetTotalAmountByCategoryAsync(Guid categoryId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAmountByAccountAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    // Monthly/Yearly aggregations
    Task<Dictionary<string, decimal>> GetMonthlyIncomeByUserIdAsync(Guid userId, int year, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetMonthlyExpensesByUserIdAsync(Guid userId, int year, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCategoryExpensesByUserIdAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    // Date-specific queries
    Task<IEnumerable<Transaction>> GetTransactionsForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetTransactionsForMonthAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetTransactionsForYearAsync(int year, CancellationToken cancellationToken = default);
    
    // Statistics
    Task<int> GetTransactionCountByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageTransactionAmountAsync(Guid userId, TransactionType? transactionType = null, CancellationToken cancellationToken = default);
    Task<Transaction?> GetLargestTransactionAsync(Guid userId, TransactionType transactionType, CancellationToken cancellationToken = default);
    
    // Bulk operations
    Task<IEnumerable<Transaction>> BulkUpdateCategoryAsync(Guid oldCategoryId, Guid newCategoryId, CancellationToken cancellationToken = default);
    Task<int> BulkDeleteByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<int> BulkProcessPendingTransactionsAsync(CancellationToken cancellationToken = default);
}