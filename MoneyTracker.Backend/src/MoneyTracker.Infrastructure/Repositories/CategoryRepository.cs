using Microsoft.EntityFrameworkCore;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.Common.Models;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetByCategoryTypeAsync(CategoryType categoryType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CategoryType == categoryType && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetParentCategoriesAsync(CategoryType? categoryType = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.ParentCategoryId == null && c.IsActive);
        
        if (categoryType.HasValue)
        {
            query = query.Where(c => c.CategoryType == categoryType.Value);
        }
        
        return await query
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetSystemCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsSystemCategory)
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetDefaultCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsDefault)
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(string name, CategoryType categoryType, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            c => (c.Name == name || c.NameGerman == name) && c.CategoryType == categoryType, 
            cancellationToken);
    }

    public async Task<IEnumerable<Category>> SearchByKeywordsAsync(string keywords, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keywords))
            return new List<Category>();
        
        var searchTerm = keywords.ToLower();
        
        return await _dbSet
            .Where(c => c.IsActive && 
                   (c.Keywords != null && c.Keywords.ToLower().Contains(searchTerm) ||
                    c.Name.ToLower().Contains(searchTerm) ||
                    (c.NameGerman != null && c.NameGerman.ToLower().Contains(searchTerm))))
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> FindCategoryByKeywordsAsync(string text, CategoryType categoryType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        
        var searchText = text.ToLower();
        
        var categories = await _dbSet
            .Where(c => c.CategoryType == categoryType && c.IsActive && c.Keywords != null)
            .ToListAsync(cancellationToken);
            
        return categories.FirstOrDefault(c => c.MatchesKeywords(text));
    }

    public async Task<IEnumerable<Category>> GetCategoriesWithBudgetAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.BudgetLimit.HasValue && c.IsActive)
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetOverBudgetCategoriesAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        // This requires a more complex query that joins with transactions
        // For now, we'll implement it by loading categories with budgets and checking each one
        var categoriesWithBudgets = await GetCategoriesWithBudgetAsync(cancellationToken);
        var overBudgetCategories = new List<Category>();
        
        foreach (var category in categoriesWithBudgets)
        {
            if (category.IsOverBudget(fromDate, toDate))
            {
                overBudgetCategories.Add(category);
            }
        }
        
        return overBudgetCategories;
    }

    public async Task<bool> HasTransactionsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Transaction>()
            .AnyAsync(t => t.CategoryId == categoryId, cancellationToken);
    }

    public async Task<bool> HasSubCategoriesAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken);
    }

    public async Task<int> GetTransactionCountByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Transaction>()
            .CountAsync(t => t.CategoryId == categoryId, cancellationToken);
    }

    // New methods implementation
    public async Task<PagedResult<Category>> GetUserCategoriesPagedAsync(Guid userId, CategoryQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.UserId == userId || c.IsSystemCategory);

        // Apply filters
        if (parameters.CategoryType.HasValue)
            query = query.Where(c => c.CategoryType == parameters.CategoryType.Value);

        if (parameters.IsActive.HasValue)
            query = query.Where(c => c.IsActive == parameters.IsActive.Value);

        if (parameters.IsSystemCategory.HasValue)
            query = query.Where(c => c.IsSystemCategory == parameters.IsSystemCategory.Value);

        if (parameters.ParentCategoryId.HasValue)
            query = query.Where(c => c.ParentCategoryId == parameters.ParentCategoryId.Value);

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            var searchLower = parameters.SearchTerm.ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(searchLower) ||
                (c.NameGerman != null && c.NameGerman.ToLower().Contains(searchLower)) ||
                (c.Description != null && c.Description.ToLower().Contains(searchLower)));
        }

        // Apply sorting
        query = parameters.SortBy?.ToLower() switch
        {
            "name" => parameters.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "categorytype" => parameters.SortDescending ? query.OrderByDescending(c => c.CategoryType) : query.OrderBy(c => c.CategoryType),
            "createdat" => parameters.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.CategoryType).ThenBy(c => c.SortOrder).ThenBy(c => c.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Include(c => c.ParentCategory)
            .ToListAsync(cancellationToken);

        return new PagedResult<Category>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };
    }

    public async Task<List<Category>> GetUserCategoriesWithSystemAsync(Guid userId, CategoryType? categoryType = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => (c.UserId == userId || c.IsSystemCategory) && c.IsActive);
        
        if (categoryType.HasValue)
            query = query.Where(c => c.CategoryType == categoryType.Value);
            
        return await query
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetCategoriesByTypeAsync(CategoryType categoryType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CategoryType == categoryType && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryUsageStatsDto> GetCategoryUsageStatsAsync(Guid categoryId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var category = await GetByIdAsync(categoryId, cancellationToken);
        if (category == null)
            throw new ArgumentException($"Category {categoryId} not found");

        var query = _context.Set<Transaction>().Where(t => t.CategoryId == categoryId);
        
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        var transactions = await query.ToListAsync(cancellationToken);
        
        if (!transactions.Any())
        {
            return new CategoryUsageStatsDto
            {
                CategoryId = categoryId,
                CategoryName = category.DisplayName,
                TransactionCount = 0,
                TotalAmount = 0,
                AverageAmount = 0,
                MinAmount = 0,
                MaxAmount = 0
            };
        }

        var monthlyGroups = transactions
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new MonthlyUsageDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TransactionCount = g.Count(),
                TotalAmount = g.Sum(t => t.Amount)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        return new CategoryUsageStatsDto
        {
            CategoryId = categoryId,
            CategoryName = category.DisplayName,
            TransactionCount = transactions.Count,
            TotalAmount = transactions.Sum(t => t.Amount),
            AverageAmount = transactions.Average(t => t.Amount),
            MinAmount = transactions.Min(t => t.Amount),
            MaxAmount = transactions.Max(t => t.Amount),
            FirstTransactionDate = transactions.Min(t => t.TransactionDate),
            LastTransactionDate = transactions.Max(t => t.TransactionDate),
            MonthlyUsage = monthlyGroups
        };
    }

    public async Task<List<Category>> GetCategoriesWithTransactionStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.UserId == userId || c.IsSystemCategory)
            .Include(c => c.Transactions)
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Id == categoryId, cancellationToken);
    }

    public async Task<bool> IsUserCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Id == categoryId && (c.UserId == userId || c.IsSystemCategory), cancellationToken);
    }

    public async Task<int> BulkUpdateAsync(List<Guid> categoryIds, UpdateCategoryDto updates, CancellationToken cancellationToken = default)
    {
        var categories = await _dbSet
            .Where(c => categoryIds.Contains(c.Id) && !c.IsSystemCategory)
            .ToListAsync(cancellationToken);

        var updatedCount = 0;
        foreach (var category in categories)
        {
            var hasChanges = false;
            
            if (!string.IsNullOrEmpty(updates.Name))
            {
                category.Name = updates.Name;
                hasChanges = true;
            }
            
            if (updates.Description != null)
            {
                category.Description = updates.Description;
                hasChanges = true;
            }
            
            if (!string.IsNullOrEmpty(updates.Icon))
            {
                category.Icon = updates.Icon;
                hasChanges = true;
            }
            
            if (!string.IsNullOrEmpty(updates.Color))
            {
                category.Color = updates.Color;
                hasChanges = true;
            }
            
            if (updates.DefaultVatRate.HasValue)
            {
                category.DefaultVatRate = updates.DefaultVatRate.Value;
                hasChanges = true;
            }
            
            if (updates.Keywords != null)
            {
                category.Keywords = updates.Keywords;
                hasChanges = true;
            }
            
            if (updates.IsActive.HasValue)
            {
                category.IsActive = updates.IsActive.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                category.UpdatedAt = DateTime.UtcNow;
                updatedCount++;
            }
        }

        if (updatedCount > 0)
            await _context.SaveChangesAsync(cancellationToken);
            
        return updatedCount;
    }

    public async Task<bool> CanDeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await GetByIdAsync(categoryId, cancellationToken);
        if (category == null) return false;
        
        // Cannot delete system categories
        if (category.IsSystemCategory) return false;
        
        // Cannot delete if has transactions
        var hasTransactions = await HasTransactionsAsync(categoryId, cancellationToken);
        if (hasTransactions) return false;
        
        // Cannot delete if has subcategories
        var hasSubCategories = await HasSubCategoriesAsync(categoryId, cancellationToken);
        if (hasSubCategories) return false;
        
        return true;
    }

    public async Task<int> MergeTransactionsAsync(Guid sourceCategoryId, Guid targetCategoryId, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.Set<Transaction>()
            .Where(t => t.CategoryId == sourceCategoryId)
            .ToListAsync(cancellationToken);
            
        foreach (var transaction in transactions)
        {
            transaction.CategoryId = targetCategoryId;
            transaction.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return transactions.Count;
    }
}