using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AutoMapper;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.Common.Models;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmartCategorizationService _smartCategorizationService;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;
    private readonly Dictionary<string, List<string>> _categoryKeywords;

    public CategoryService(
        IUnitOfWork unitOfWork,
        ISmartCategorizationService smartCategorizationService,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _smartCategorizationService = smartCategorizationService;
        _mapper = mapper;
        _logger = logger;
        
        // Initialize category suggestion keywords (German)
        _categoryKeywords = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Lebensmittel"] = new() { "supermarkt", "rewe", "edeka", "aldi", "lidl", "kaufland", "penny", "netto", "real", "dm", "rossmann" },
            ["Tankstelle"] = new() { "tankstelle", "shell", "aral", "esso", "bp", "total", "jet", "benzin", "diesel" },
            ["Restaurant"] = new() { "restaurant", "mcdonalds", "burger king", "kfc", "subway", "pizza", "döner", "imbiss", "cafe", "starbucks" },
            ["Transport"] = new() { "bahn", "bus", "taxi", "uber", "deutsche bahn", "hvv", "mvg", "bvg", "tickets", "fahrkarte" },
            ["Kleidung"] = new() { "h&m", "zara", "c&a", "primark", "zalando", "otto", "peek", "about you", "kleidung", "schuhe" },
            ["Elektronik"] = new() { "media markt", "saturn", "amazon", "apple", "samsung", "conrad", "cyberport", "notebooksbilliger" },
            ["Gesundheit"] = new() { "apotheke", "arzt", "zahnarzt", "krankenhaus", "medikamente", "brille", "optiker" },
            ["Wohnen"] = new() { "miete", "nebenkosten", "strom", "gas", "wasser", "internet", "telekom", "vodafone", "1&1", "o2" },
            ["Versicherung"] = new() { "versicherung", "allianz", "axa", "generali", "huk", "devk", "signal iduna" },
            ["Bank"] = new() { "bank", "gebühren", "kontoführung", "zinsen", "kredit", "darlehen" },
            ["Unterhaltung"] = new() { "kino", "theater", "netflix", "spotify", "amazon prime", "disney+", "sky", "konzert", "event" },
            ["Sport"] = new() { "fitnessstudio", "mcfit", "clever fit", "fitness first", "sport", "training" },
            ["Bildung"] = new() { "schule", "universität", "kurs", "seminar", "buch", "bücher", "udemy", "coursera" },
            ["Geschenke"] = new() { "geschenk", "blumen", "schmuck", "spielwaren", "toys", "present" },
            ["Reisen"] = new() { "hotel", "booking", "airbnb", "flug", "lufthansa", "ryanair", "eurowings", "bahn", "reise" },
            ["Steuern"] = new() { "finanzamt", "steuer", "kirchensteuer", "solidaritätszuschlag", "tax" },
            ["Gehalt"] = new() { "gehalt", "lohn", "salary", "bonus", "prämie", "arbeitgeber" },
            ["Zinsen"] = new() { "zinsen", "dividende", "kapitalerträge", "zinsgutschrift" }
        };
    }

    public async Task<PagedResult<CategoryResponseDto>> GetCategoriesAsync(Guid userId, CategoryQueryParameters parameters)
    {
        try
        {
            var pagedCategories = await _unitOfWork.Categories.GetUserCategoriesPagedAsync(userId, parameters);
            var categoryDtos = new List<CategoryResponseDto>();

            foreach (var category in pagedCategories.Items)
            {
                var dto = _mapper.Map<CategoryResponseDto>(category);
                dto.CategoryTypeName = GetCategoryTypeName(category.CategoryType);
                dto.TransactionCount = await _unitOfWork.Categories.GetTransactionCountByCategoryAsync(category.Id);
                categoryDtos.Add(dto);
            }

            return new PagedResult<CategoryResponseDto>
            {
                Items = categoryDtos,
                TotalCount = pagedCategories.TotalCount,
                Page = pagedCategories.Page,
                PageSize = pagedCategories.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories for user {UserId}", userId);
            throw;
        }
    }

    public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid userId, Guid categoryId)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null) return null;

            // Check if user has access to this category
            if (!category.IsSystemCategory && category.UserId != userId)
                return null;

            var dto = _mapper.Map<CategoryResponseDto>(category);
            dto.CategoryTypeName = GetCategoryTypeName(category.CategoryType);
            dto.TransactionCount = await _unitOfWork.Categories.GetTransactionCountByCategoryAsync(category.Id);
            
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId} for user {UserId}", categoryId, userId);
            throw;
        }
    }

    public async Task<CategoryResponseDto> CreateCategoryAsync(Guid userId, CreateCategoryDto dto)
    {
        try
        {
            // Validate parent category if provided
            if (dto.ParentCategoryId.HasValue)
            {
                var parentCategory = await _unitOfWork.Categories.GetByIdAsync(dto.ParentCategoryId.Value);
                if (parentCategory == null || parentCategory.CategoryType != dto.CategoryType)
                    throw new ArgumentException("Invalid parent category");
                
                // Check if parent belongs to user or is system category
                if (!parentCategory.IsSystemCategory && parentCategory.UserId != userId)
                    throw new UnauthorizedAccessException("Parent category not accessible");
            }

            // Validate VAT rate
            if (dto.DefaultVatRate < 0 || dto.DefaultVatRate > 1)
                throw new ArgumentException("VAT rate must be between 0 and 1");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                CategoryType = dto.CategoryType,
                Icon = dto.Icon,
                Color = dto.Color,
                ParentCategoryId = dto.ParentCategoryId,
                DefaultVatRate = dto.DefaultVatRate,
                Keywords = dto.Keywords,
                IsSystemCategory = false,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<CategoryResponseDto>(category);
            result.CategoryTypeName = GetCategoryTypeName(category.CategoryType);
            
            _logger.LogInformation("Created category {CategoryName} for user {UserId}", category.Name, userId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category for user {UserId}", userId);
            throw;
        }
    }

    public async Task<CategoryResponseDto?> UpdateCategoryAsync(Guid userId, Guid categoryId, UpdateCategoryDto dto)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null) return null;

            // Check if user can modify this category
            if (category.IsSystemCategory || (!category.IsSystemCategory && category.UserId != userId))
                throw new UnauthorizedAccessException("Cannot modify this category");

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.Name))
                category.Name = dto.Name;
                
            if (dto.Description != null)
                category.Description = dto.Description;
                
            if (!string.IsNullOrEmpty(dto.Icon))
                category.Icon = dto.Icon;
                
            if (!string.IsNullOrEmpty(dto.Color))
                category.Color = dto.Color;

            if (dto.ParentCategoryId.HasValue)
            {
                var parentCategory = await _unitOfWork.Categories.GetByIdAsync(dto.ParentCategoryId.Value);
                if (parentCategory == null || parentCategory.CategoryType != category.CategoryType)
                    throw new ArgumentException("Invalid parent category");
                    
                category.ParentCategoryId = dto.ParentCategoryId;
            }

            if (dto.DefaultVatRate.HasValue)
            {
                if (dto.DefaultVatRate.Value < 0 || dto.DefaultVatRate.Value > 1)
                    throw new ArgumentException("VAT rate must be between 0 and 1");
                category.DefaultVatRate = dto.DefaultVatRate.Value;
            }

            if (dto.Keywords != null)
                category.Keywords = dto.Keywords;
                
            if (dto.IsActive.HasValue)
                category.IsActive = dto.IsActive.Value;

            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<CategoryResponseDto>(category);
            result.CategoryTypeName = GetCategoryTypeName(category.CategoryType);
            result.TransactionCount = await _unitOfWork.Categories.GetTransactionCountByCategoryAsync(category.Id);
            
            _logger.LogInformation("Updated category {CategoryId} for user {UserId}", categoryId, userId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId} for user {UserId}", categoryId, userId);
            throw;
        }
    }

    public async Task<bool> DeleteCategoryAsync(Guid userId, Guid categoryId)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null) return false;

            // Check if user can delete this category
            if (category.IsSystemCategory || (!category.IsSystemCategory && category.UserId != userId))
                throw new UnauthorizedAccessException("Cannot delete this category");

            // Check if category can be safely deleted
            var canDelete = await _unitOfWork.Categories.CanDeleteCategoryAsync(categoryId);
            if (!canDelete)
                throw new InvalidOperationException("Category cannot be deleted as it has transactions or subcategories");

            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Deleted category {CategoryId} for user {UserId}", categoryId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId} for user {UserId}", categoryId, userId);
            throw;
        }
    }

    public async Task<List<CategoryHierarchyDto>> GetCategoryHierarchyAsync(Guid userId, CategoryType? categoryType)
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetUserCategoriesWithSystemAsync(userId, categoryType);
            
            var hierarchyGroups = categories
                .GroupBy(c => c.CategoryType)
                .Select(group => new CategoryHierarchyDto
                {
                    CategoryType = group.Key,
                    CategoryTypeName = GetCategoryTypeName(group.Key),
                    Categories = BuildCategoryTree(group.Where(c => !c.ParentCategoryId.HasValue).ToList(), group.ToList())
                })
                .ToList();

            return hierarchyGroups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category hierarchy for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<CategorySuggestionDto>> SuggestCategoryAsync(string description, string? merchantName, decimal? amount, TransactionType transactionType)
    {
        try
        {
            return await _smartCategorizationService.SuggestCategoriesAsync(description, merchantName, amount, transactionType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting category for description: {Description}", description);
            return new List<CategorySuggestionDto>();
        }
    }

    public async Task<CategoryUsageStatsDto> GetCategoryUsageStatsAsync(Guid userId, Guid categoryId, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
                throw new ArgumentException("Category not found");

            // Check access
            if (!category.IsSystemCategory && category.UserId != userId)
                throw new UnauthorizedAccessException("Access denied");

            return await _unitOfWork.Categories.GetCategoryUsageStatsAsync(categoryId, startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage stats for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<bool> BulkUpdateCategoriesAsync(Guid userId, BulkUpdateCategoriesDto dto)
    {
        try
        {
            // Validate that user owns all categories
            foreach (var categoryId in dto.CategoryIds)
            {
                var isUserCategory = await _unitOfWork.Categories.IsUserCategoryAsync(categoryId, userId);
                if (!isUserCategory)
                    throw new UnauthorizedAccessException($"Access denied for category {categoryId}");
            }

            var updatedCount = await _unitOfWork.Categories.BulkUpdateAsync(dto.CategoryIds, dto.Updates);
            
            _logger.LogInformation("Bulk updated {Count} categories for user {UserId}", updatedCount, userId);
            return updatedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating categories for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<CategoryResponseDto>> ImportCategoriesAsync(Guid userId, ImportCategoriesDto dto)
    {
        try
        {
            var importedCategories = new List<CategoryResponseDto>();

            foreach (var item in dto.Categories)
            {
                // Check if category already exists
                var existingCategory = await _unitOfWork.Categories.GetByNameAsync(item.Name, item.CategoryType);
                if (existingCategory != null && !dto.OverwriteExisting)
                {
                    _logger.LogWarning("Category {CategoryName} already exists, skipping", item.Name);
                    continue;
                }

                // Resolve parent category if provided
                Guid? parentCategoryId = null;
                if (!string.IsNullOrEmpty(item.ParentCategoryName))
                {
                    var parentCategory = await _unitOfWork.Categories.GetByNameAsync(item.ParentCategoryName, item.CategoryType);
                    parentCategoryId = parentCategory?.Id;
                }

                var createDto = new CreateCategoryDto
                {
                    Name = item.Name,
                    Description = item.Description,
                    CategoryType = item.CategoryType,
                    Icon = item.Icon,
                    Color = item.Color,
                    ParentCategoryId = parentCategoryId,
                    DefaultVatRate = item.DefaultVatRate,
                    Keywords = item.Keywords,
                    IsActive = item.IsActive
                };

                var imported = await CreateCategoryAsync(userId, createDto);
                importedCategories.Add(imported);
            }

            _logger.LogInformation("Imported {Count} categories for user {UserId}", importedCategories.Count, userId);
            return importedCategories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing categories for user {UserId}", userId);
            throw;
        }
    }

    public async Task<byte[]> ExportCategoriesAsync(Guid userId, CategoryType? categoryType)
    {
        try
        {
            var parameters = new CategoryQueryParameters
            {
                CategoryType = categoryType,
                PageSize = int.MaxValue
            };

            var categories = await GetCategoriesAsync(userId, parameters);
            var json = JsonSerializer.Serialize(categories.Items, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return System.Text.Encoding.UTF8.GetBytes(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting categories for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> MergeCategoriesAsync(Guid userId, Guid sourceCategoryId, Guid targetCategoryId)
    {
        try
        {
            var sourceCategory = await _unitOfWork.Categories.GetByIdAsync(sourceCategoryId);
            var targetCategory = await _unitOfWork.Categories.GetByIdAsync(targetCategoryId);

            if (sourceCategory == null || targetCategory == null)
                throw new ArgumentException("One or both categories not found");

            // Validate user access
            if ((!sourceCategory.IsSystemCategory && sourceCategory.UserId != userId) ||
                (!targetCategory.IsSystemCategory && targetCategory.UserId != userId))
                throw new UnauthorizedAccessException("Access denied");

            // Ensure categories are of the same type
            if (sourceCategory.CategoryType != targetCategory.CategoryType)
                throw new ArgumentException("Categories must be of the same type");

            // Move all transactions from source to target
            var movedCount = await _unitOfWork.Categories.MergeTransactionsAsync(sourceCategoryId, targetCategoryId);
            
            // Delete source category
            _unitOfWork.Categories.Delete(sourceCategory);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Merged category {SourceId} into {TargetId}, moved {TransactionCount} transactions", 
                sourceCategoryId, targetCategoryId, movedCount);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging categories {SourceId} -> {TargetId}", sourceCategoryId, targetCategoryId);
            throw;
        }
    }

    // Legacy methods for backward compatibility
    public async Task<Category?> SuggestCategoryAsync(string description, string? merchantName, decimal amount, TransactionType transactionType)
    {
        try
        {
            var suggestions = await _smartCategorizationService.SuggestCategoriesAsync(description, merchantName, amount, transactionType);
            if (suggestions.Any())
            {
                var topSuggestion = suggestions.First();
                return await _unitOfWork.Categories.GetByIdAsync(topSuggestion.CategoryId);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in legacy suggest category method");
            return null;
        }
    }

    public async Task<List<Category>> GetUserCategoriesAsync(Guid userId)
    {
        try
        {
            return await _unitOfWork.Categories.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user categories for {UserId}", userId);
            return new List<Category>();
        }
    }

    public async Task<Category?> GetByIdAsync(Guid categoryId)
    {
        try
        {
            return await _unitOfWork.Categories.GetByIdAsync(categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", categoryId);
            return null;
        }
    }

    #region Helper Methods

    private List<CategoryResponseDto> BuildCategoryTree(List<Category> rootCategories, List<Category> allCategories)
    {
        var categoryDtos = new List<CategoryResponseDto>();

        foreach (var rootCategory in rootCategories)
        {
            var categoryDto = _mapper.Map<CategoryResponseDto>(rootCategory);
            categoryDto.CategoryTypeName = GetCategoryTypeName(rootCategory.CategoryType);
            categoryDto.SubCategories = BuildCategoryTree(
                allCategories.Where(c => c.ParentCategoryId == rootCategory.Id).ToList(),
                allCategories
            );
            categoryDtos.Add(categoryDto);
        }

        return categoryDtos;
    }

    private string GetCategoryTypeName(CategoryType categoryType)
    {
        return categoryType switch
        {
            CategoryType.Income => "Einnahmen",
            CategoryType.Expense => "Ausgaben",
            _ => "Unbekannt"
        };
    }

    #endregion
}