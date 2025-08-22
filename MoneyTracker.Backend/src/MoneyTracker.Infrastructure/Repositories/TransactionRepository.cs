using Microsoft.EntityFrameworkCore;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(DbContext context) : base(context)
    {
    }

    // User-specific queries
    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && 
                   t.TransactionDate >= startDate && 
                   t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetRecentByUserIdAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    // Account-specific queries
    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAndDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.AccountId == accountId && 
                   t.TransactionDate >= startDate && 
                   t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    // Category-specific queries
    public async Task<IEnumerable<Transaction>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByCategoryIdAndDateRangeAsync(Guid categoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.CategoryId == categoryId && 
                   t.TransactionDate >= startDate && 
                   t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    // Transaction type queries
    public async Task<IEnumerable<Transaction>> GetByTransactionTypeAsync(TransactionType transactionType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.TransactionType == transactionType)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetIncomeByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.UserId == userId && t.TransactionType == TransactionType.Income);
        
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        
        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetExpensesByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.UserId == userId && t.TransactionType == TransactionType.Expense);
        
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        
        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    // Search and filtering
    public async Task<IEnumerable<Transaction>> SearchTransactionsAsync(Guid userId, string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Transaction>();
        
        var search = searchTerm.ToLower();
        
        return await _dbSet
            .Where(t => t.UserId == userId && 
                   (t.Description.ToLower().Contains(search) ||
                    (t.MerchantName != null && t.MerchantName.ToLower().Contains(search)) ||
                    (t.Notes != null && t.Notes.ToLower().Contains(search)) ||
                    (t.ReferenceNumber != null && t.ReferenceNumber.ToLower().Contains(search))))
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByMerchantNameAsync(string merchantName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.MerchantName == merchantName)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Tags != null && t.Tags.Contains(tag))
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByPaymentMethodAsync(string paymentMethod, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.PaymentMethod == paymentMethod)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    // Status-based queries
    public async Task<IEnumerable<Transaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsPending)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetUnverifiedTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => !t.IsVerified)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetRecurringTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsRecurring)
            .OrderBy(t => t.RecurrenceGroupId)
            .ThenByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    // Import-related queries
    public async Task<IEnumerable<Transaction>> GetByImportSourceAsync(string importSource, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.ImportSource == importSource)
            .OrderByDescending(t => t.ImportDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByExternalTransactionIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.ExternalTransactionId == externalId, cancellationToken);
    }

    public async Task<bool> ExternalTransactionExistsAsync(string externalId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(t => t.ExternalTransactionId == externalId, cancellationToken);
    }

    // Aggregation queries
    public async Task<decimal> GetTotalIncomeByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.UserId == userId && t.TransactionType == TransactionType.Income);
        
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        
        return await query.SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalExpensesByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.UserId == userId && t.TransactionType == TransactionType.Expense);
        
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        
        return await query.SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<decimal> GetNetBalanceByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var income = await GetTotalIncomeByUserIdAsync(userId, startDate, endDate, cancellationToken);
        var expenses = await GetTotalExpensesByUserIdAsync(userId, startDate, endDate, cancellationToken);
        return income - expenses;
    }

    public async Task<decimal> GetTotalAmountByCategoryAsync(Guid categoryId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.CategoryId == categoryId);
        
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        
        return await query.SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalAmountByAccountAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.AccountId == accountId);
        
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        
        return await query.SumAsync(t => t.TransactionType == TransactionType.Income ? t.Amount : -t.Amount, cancellationToken);
    }

    // Monthly/Yearly aggregations
    public async Task<Dictionary<string, decimal>> GetMonthlyIncomeByUserIdAsync(Guid userId, int year, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Where(t => t.UserId == userId && 
                   t.TransactionType == TransactionType.Income && 
                   t.TransactionDate.Year == year)
            .GroupBy(t => t.TransactionDate.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.Month.ToString("D2"), x => x.Total, cancellationToken);
        
        // Ensure all months are represented
        var monthlyIncome = new Dictionary<string, decimal>();
        for (int i = 1; i <= 12; i++)
        {
            var monthKey = i.ToString("D2");
            monthlyIncome[monthKey] = result.ContainsKey(monthKey) ? result[monthKey] : 0;
        }
        
        return monthlyIncome;
    }

    public async Task<Dictionary<string, decimal>> GetMonthlyExpensesByUserIdAsync(Guid userId, int year, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Where(t => t.UserId == userId && 
                   t.TransactionType == TransactionType.Expense && 
                   t.TransactionDate.Year == year)
            .GroupBy(t => t.TransactionDate.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.Month.ToString("D2"), x => x.Total, cancellationToken);
        
        // Ensure all months are represented
        var monthlyExpenses = new Dictionary<string, decimal>();
        for (int i = 1; i <= 12; i++)
        {
            var monthKey = i.ToString("D2");
            monthlyExpenses[monthKey] = result.ContainsKey(monthKey) ? result[monthKey] : 0;
        }
        
        return monthlyExpenses;
    }

    public async Task<Dictionary<string, decimal>> GetCategoryExpensesByUserIdAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && 
                   t.TransactionType == TransactionType.Expense &&
                   t.TransactionDate >= startDate && 
                   t.TransactionDate <= endDate)
            .GroupBy(t => t.Category.DisplayName)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(t => t.Amount), cancellationToken);
    }

    // Date-specific queries
    public async Task<IEnumerable<Transaction>> GetTransactionsForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.TransactionDate.Date == date.Date)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsForMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.TransactionDate.Year == year && t.TransactionDate.Month == month)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsForYearAsync(int year, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.TransactionDate.Year == year)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    // Statistics
    public async Task<int> GetTransactionCountByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.UserId == userId);
        
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        
        return await query.CountAsync(cancellationToken);
    }

    public async Task<decimal> GetAverageTransactionAmountAsync(Guid userId, TransactionType? transactionType = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.UserId == userId);
        
        if (transactionType.HasValue)
            query = query.Where(t => t.TransactionType == transactionType.Value);
        
        return await query.AverageAsync(t => t.Amount, cancellationToken);
    }

    public async Task<Transaction?> GetLargestTransactionAsync(Guid userId, TransactionType transactionType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.TransactionType == transactionType)
            .OrderByDescending(t => t.Amount)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // Bulk operations
    public async Task<IEnumerable<Transaction>> BulkUpdateCategoryAsync(Guid oldCategoryId, Guid newCategoryId, CancellationToken cancellationToken = default)
    {
        var transactions = await _dbSet
            .Where(t => t.CategoryId == oldCategoryId)
            .ToListAsync(cancellationToken);
        
        foreach (var transaction in transactions)
        {
            transaction.CategoryId = newCategoryId;
        }
        
        return transactions;
    }

    public async Task<int> BulkDeleteByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var transactions = await _dbSet
            .Where(t => t.AccountId == accountId)
            .ToListAsync(cancellationToken);
        
        _dbSet.RemoveRange(transactions);
        return transactions.Count;
    }

    public async Task<int> BulkProcessPendingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var pendingTransactions = await _dbSet
            .Where(t => t.IsPending)
            .ToListAsync(cancellationToken);
        
        foreach (var transaction in pendingTransactions)
        {
            transaction.Process();
        }
        
        return pendingTransactions.Count;
    }
}