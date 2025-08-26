using Microsoft.EntityFrameworkCore;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.Common.Models;
using MoneyTracker.Application.DTOs.Transaction;
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

    public async Task<IEnumerable<Transaction>> GetByPaymentMethodAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
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

    public async Task<List<Transaction>> GetByIdsAsync(Guid userId, List<Guid> transactionIds)
    {
        return await _dbSet
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && transactionIds.Contains(t.Id))
            .ToListAsync();
    }

    public async Task<int> BulkDeleteAsync(Guid userId, List<Guid> transactionIds)
    {
        var transactions = await _dbSet
            .Where(t => t.UserId == userId && transactionIds.Contains(t.Id))
            .ToListAsync();

        if (!transactions.Any())
            return 0;

        _dbSet.RemoveRange(transactions);
        await _context.SaveChangesAsync();

        return transactions.Count;
    }

    public async Task<TransactionSummaryDto> GetSummaryDataAsync(Guid userId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _dbSet
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(t => t.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.Date <= toDate.Value);

        var transactions = await query.ToListAsync();

        var incomeTransactions = transactions.Where(t => t.Type == TransactionType.Income).ToList();
        var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense).ToList();

        var summary = new TransactionSummaryDto
        {
            TotalIncome = incomeTransactions.Sum(t => t.Amount),
            TotalExpenses = expenseTransactions.Sum(t => t.Amount),
            TotalVat = transactions.Where(t => t.VatAmount > 0).Sum(t => t.VatAmount),
            TotalTransactionCount = transactions.Count,
            IncomeTransactionCount = incomeTransactions.Count,
            ExpenseTransactionCount = expenseTransactions.Count,
            FirstTransactionDate = transactions.Any() ? transactions.Min(t => t.Date) : null,
            LastTransactionDate = transactions.Any() ? transactions.Max(t => t.Date) : null
        };

        // Monthly breakdown
        summary.MonthlySummaries = transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new MonthlySummaryDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalIncome = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                TotalExpenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        // Category breakdown
        summary.CategorySummaries = transactions
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color, t.Type })
            .Select(g => new CategorySummaryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalAmount = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .ToList();

        // Calculate percentages
        var totalByType = summary.CategorySummaries
            .GroupBy(c => c.CategoryId)
            .ToDictionary(g => g.Key, g => g.Sum(c => c.TotalAmount));

        foreach (var categoryData in summary.CategorySummaries)
        {
            var typeTotal = totalByType.GetValueOrDefault(categoryData.CategoryId, 1); // Avoid division by zero
            categoryData.Percentage = typeTotal > 0 ? (categoryData.TotalAmount / typeTotal) * 100 : 0;
        }

        return summary;
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid transactionId)
    {
        return await _dbSet
            .AnyAsync(t => t.Id == transactionId && t.UserId == userId);
    }

    private static IQueryable<Transaction> ApplyFilters(IQueryable<Transaction> query, TransactionQueryParameters parameters)
    {
        if (parameters.FromDate.HasValue)
            query = query.Where(t => t.Date >= parameters.FromDate.Value);

        if (parameters.ToDate.HasValue)
            query = query.Where(t => t.Date <= parameters.ToDate.Value);

        if (parameters.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == parameters.CategoryId.Value);

        if (parameters.Type.HasValue)
            query = query.Where(t => t.Type == parameters.Type.Value);

        if (parameters.PaymentMethod.HasValue)
            query = query.Where(t => t.PaymentMethod == parameters.PaymentMethod);

        if (parameters.MinAmount.HasValue)
            query = query.Where(t => t.Amount >= parameters.MinAmount.Value);

        if (parameters.MaxAmount.HasValue)
            query = query.Where(t => t.Amount <= parameters.MaxAmount.Value);

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Description.ToLower().Contains(searchTerm) ||
                (t.Supplier != null && t.Supplier.ToLower().Contains(searchTerm)) ||
                (t.InvoiceNumber != null && t.InvoiceNumber.ToLower().Contains(searchTerm)) ||
                (t.Notes != null && t.Notes.ToLower().Contains(searchTerm)));
        }

        return query;
    }

    private static IQueryable<Transaction> ApplySorting(IQueryable<Transaction> query, string? sortBy, string? sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "date" => isDescending ? query.OrderByDescending(t => t.Date) : query.OrderBy(t => t.Date),
            "amount" => isDescending ? query.OrderByDescending(t => t.Amount) : query.OrderBy(t => t.Amount),
            "description" => isDescending ? query.OrderByDescending(t => t.Description) : query.OrderBy(t => t.Description),
            "category" => isDescending ? query.OrderByDescending(t => t.Category.Name) : query.OrderBy(t => t.Category.Name),
            "type" => isDescending ? query.OrderByDescending(t => t.Type) : query.OrderBy(t => t.Type),
            "createdat" => isDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.Date) // Default sort
        };
    }
}