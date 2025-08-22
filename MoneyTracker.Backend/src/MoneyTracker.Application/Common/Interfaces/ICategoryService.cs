using MoneyTracker.Application.Common.Models;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<PagedResult<CategoryResponseDto>> GetCategoriesAsync(Guid userId, CategoryQueryParameters parameters);
    Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid userId, Guid categoryId);
    Task<CategoryResponseDto> CreateCategoryAsync(Guid userId, CreateCategoryDto dto);
    Task<CategoryResponseDto?> UpdateCategoryAsync(Guid userId, Guid categoryId, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(Guid userId, Guid categoryId);
    Task<List<CategoryHierarchyDto>> GetCategoryHierarchyAsync(Guid userId, CategoryType? categoryType);
    Task<List<CategorySuggestionDto>> SuggestCategoryAsync(string description, string? merchantName, decimal? amount, TransactionType transactionType);
    Task<CategoryUsageStatsDto> GetCategoryUsageStatsAsync(Guid userId, Guid categoryId, DateTime? startDate, DateTime? endDate);
    Task<bool> BulkUpdateCategoriesAsync(Guid userId, BulkUpdateCategoriesDto dto);
    Task<List<CategoryResponseDto>> ImportCategoriesAsync(Guid userId, ImportCategoriesDto dto);
    Task<byte[]> ExportCategoriesAsync(Guid userId, CategoryType? categoryType);
    Task<bool> MergeCategoriesAsync(Guid userId, Guid sourceCategoryId, Guid targetCategoryId);
    Task<List<Category>> GetUserCategoriesAsync(Guid userId);
    Task<Category?> GetByIdAsync(Guid categoryId);
    Task<Category?> SuggestCategoryAsync(string description, string? merchantName, decimal amount, TransactionType transactionType);
}