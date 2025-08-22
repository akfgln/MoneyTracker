using MoneyTracker.Application.Common.Models;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    // Existing methods
    Task<IEnumerable<Category>> GetByCategoryTypeAsync(CategoryType categoryType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetParentCategoriesAsync(CategoryType? categoryType = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetSystemCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetDefaultCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByNameAsync(string name, CategoryType categoryType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> SearchByKeywordsAsync(string keywords, CancellationToken cancellationToken = default);
    Task<Category?> FindCategoryByKeywordsAsync(string text, CategoryType categoryType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetCategoriesWithBudgetAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetOverBudgetCategoriesAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<bool> HasTransactionsAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> HasSubCategoriesAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<int> GetTransactionCountByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    
    // New methods for comprehensive category management
    Task<PagedResult<Category>> GetUserCategoriesPagedAsync(Guid userId, CategoryQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<List<Category>> GetUserCategoriesWithSystemAsync(Guid userId, CategoryType? categoryType = null, CancellationToken cancellationToken = default);
    Task<List<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Category>> GetCategoriesByTypeAsync(CategoryType categoryType, CancellationToken cancellationToken = default);
    Task<CategoryUsageStatsDto> GetCategoryUsageStatsAsync(Guid categoryId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<List<Category>> GetCategoriesWithTransactionStatsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> IsUserCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> BulkUpdateAsync(List<Guid> categoryIds, UpdateCategoryDto updates, CancellationToken cancellationToken = default);
    Task<bool> CanDeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<int> MergeTransactionsAsync(Guid sourceCategoryId, Guid targetCategoryId, CancellationToken cancellationToken = default);
}