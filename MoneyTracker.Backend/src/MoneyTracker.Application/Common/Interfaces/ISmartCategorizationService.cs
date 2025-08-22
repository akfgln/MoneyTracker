using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Interfaces;

public interface ISmartCategorizationService
{
    Task<List<CategorySuggestionDto>> SuggestCategoriesAsync(string description, string? merchantName, decimal? amount, TransactionType transactionType);
    Task LearnFromUserChoiceAsync(Guid userId, string description, string? merchantName, Guid chosenCategoryId);
    Task<Dictionary<string, double>> GetKeywordWeightsForCategoryAsync(Guid categoryId);
    Task<List<CategorySuggestionDto>> GetTopSuggestionsAsync(string searchText, TransactionType transactionType, int maxResults = 5);
    Task UpdateCategoryKeywordsAsync(Guid categoryId, List<string> newKeywords);
    Task<bool> IsGermanMerchantAsync(string merchantName);
    Task<double> CalculateCategoryConfidenceAsync(string description, string? merchantName, Guid categoryId);
}