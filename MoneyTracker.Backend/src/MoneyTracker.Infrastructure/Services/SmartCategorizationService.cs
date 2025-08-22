using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Infrastructure.Services;

public class SmartCategorizationService : ISmartCategorizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SmartCategorizationService> _logger;
    
    // German financial keywords for auto-categorization
    private readonly Dictionary<string, List<string>> _germanCategoryKeywords = new()
    {
        // Income categories
        ["salary"] = new() { "gehalt", "lohn", "salär", "vergütung", "entgelt", "bezahlung", "arbeitgeber" },
        ["freelance"] = new() { "freelance", "freiberufler", "honorar", "rechnung", "dienstleistung", "projekt" },
        ["investment"] = new() { "dividende", "zinsen", "aktien", "fonds", "anlage", "kapitalertrag", "depot" },
        ["rental"] = new() { "miete", "mieteinnahme", "vermieter", "immobilie", "wohnung", "haus" },
        
        // Expense categories
        ["housing"] = new() { "miete", "nebenkosten", "strom", "gas", "wasser", "heizung", "wohnung", "immobilie", "hausrat" },
        ["transportation"] = new() { "tankstelle", "bahn", "bus", "uber", "taxi", "auto", "benzin", "diesel", "öpnv", "mvg", "db", "lufthansa" },
        ["food"] = new() { "supermarkt", "restaurant", "café", "bäckerei", "metzgerei", "rewe", "edeka", "aldi", "lidl", "netto", "kaufland", "lieferando", "mcdonald" },
        ["healthcare"] = new() { "apotheke", "arzt", "krankenhaus", "versicherung", "medikament", "therapie", "zahnarzt", "optiker" },
        ["entertainment"] = new() { "kino", "theater", "konzert", "streaming", "netflix", "spotify", "amazon", "spiel", "sport", "fitness" },
        ["shopping"] = new() { "amazon", "zalando", "otto", "media markt", "saturn", "ikea", "h&m", "zara", "douglas", "dm", "rossmann" },
        ["education"] = new() { "schule", "universität", "kurs", "seminar", "buch", "software", "udemy", "coursera" },
        ["business"] = new() { "büro", "software", "service", "beratung", "rechnung", "steuer", "buchhaltung" }
    };

    // Common German merchants by category
    private readonly Dictionary<string, List<string>> _germanMerchants = new()
    {
        ["food"] = new() { "rewe", "edeka", "aldi", "lidl", "netto", "kaufland", "penny", "norma" },
        ["transportation"] = new() { "deutsche bahn", "db", "mvg", "bvg", "shell", "aral", "esso", "bp", "total" },
        ["shopping"] = new() { "amazon", "zalando", "otto", "h&m", "zara", "c&a", "ikea", "media markt", "saturn" },
        ["entertainment"] = new() { "netflix", "spotify", "amazon prime", "disney+", "sky", "dazn" },
        ["healthcare"] = new() { "doc morris", "shop apotheke", "zur rose", "apotheke" },
        ["utilities"] = new() { "telekom", "vodafone", "1&1", "o2", "eon", "rwe", "vattenfall" },
        ["insurance"] = new() { "allianz", "axa", "generali", "huk", "devk", "signal iduna" }
    };

    public SmartCategorizationService(
        IUnitOfWork unitOfWork,
        ILogger<SmartCategorizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<CategorySuggestionDto>> SuggestCategoriesAsync(string description, string? merchantName, decimal? amount, TransactionType transactionType)
    {
        try
        {
            var suggestions = new List<CategorySuggestionDto>();
            var categories = await _unitOfWork.Categories.GetCategoriesByTypeAsync(
                transactionType == TransactionType.Income ? CategoryType.Income : CategoryType.Expense);
            
            var searchText = $"{description} {merchantName}".ToLower();
            
            foreach (var category in categories)
            {
                var score = await CalculateCategoryConfidenceAsync(description, merchantName, category.Id);
                if (score > 0.3) // Minimum confidence threshold
                {
                    suggestions.Add(new CategorySuggestionDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.DisplayName,
                        CategoryIcon = category.Icon,
                        CategoryColor = category.Color,
                        ConfidenceScore = score,
                        MatchReason = GetMatchReason(searchText, category)
                    });
                }
            }

            // Sort by confidence score and return top 5
            return suggestions.OrderByDescending(s => s.ConfidenceScore).Take(5).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting categories for description: {Description}", description);
            return new List<CategorySuggestionDto>();
        }
    }

    public async Task<double> CalculateCategoryConfidenceAsync(string description, string? merchantName, Guid categoryId)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null) return 0.0;

            var searchText = $"{description} {merchantName}".ToLower();
            double score = 0.0;
            
            // Keyword matching from category
            if (!string.IsNullOrEmpty(category.Keywords))
            {
                var keywords = category.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var keyword in keywords)
                {
                    if (searchText.Contains(keyword.Trim().ToLower()))
                    {
                        score += 0.8;
                    }
                }
            }

            // German category keywords matching
            var categoryKey = GetCategoryKeyFromName(category.DisplayName);
            if (!string.IsNullOrEmpty(categoryKey) && _germanCategoryKeywords.ContainsKey(categoryKey))
            {
                var germanKeywords = _germanCategoryKeywords[categoryKey];
                foreach (var keyword in germanKeywords)
                {
                    if (searchText.Contains(keyword))
                    {
                        score += 0.6;
                    }
                }
            }

            // Merchant name exact matching
            var commonMerchants = GetCommonMerchantsForCategory(category.DisplayName);
            foreach (var merchant in commonMerchants)
            {
                if (searchText.Contains(merchant.ToLower()))
                {
                    score += 0.9;
                }
            }

            // Amount-based hints
            if (description.Contains("amount") && decimal.TryParse(description.Split(' ').LastOrDefault(), out var amt))
            {
                score += GetAmountBasedScore(category.DisplayName, amt);
            }

            return Math.Min(score, 1.0); // Cap at 1.0
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating confidence for category {CategoryId}", categoryId);
            return 0.0;
        }
    }

    public async Task LearnFromUserChoiceAsync(Guid userId, string description, string? merchantName, Guid chosenCategoryId)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(chosenCategoryId);
            if (category == null) return;

            var searchText = $"{description} {merchantName}".ToLower();
            var words = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            var currentKeywords = string.IsNullOrEmpty(category.Keywords) 
                ? new List<string>() 
                : category.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim())
                    .ToList();

            // Add relevant words as keywords (basic learning)
            foreach (var word in words.Where(w => w.Length > 3 && !IsCommonWord(w)))
            {
                if (!currentKeywords.Contains(word, StringComparer.OrdinalIgnoreCase))
                {
                    currentKeywords.Add(word);
                }
            }

            // Limit to 20 keywords to prevent keyword pollution
            category.Keywords = string.Join(",", currentKeywords.Take(20));
            category.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogDebug("Updated category {CategoryName} keywords based on user choice", category.DisplayName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error learning from user choice for category {CategoryId}", chosenCategoryId);
        }
    }

    public async Task<Dictionary<string, double>> GetKeywordWeightsForCategoryAsync(Guid categoryId)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null || string.IsNullOrEmpty(category.Keywords))
                return new Dictionary<string, double>();

            var keywords = category.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var weights = new Dictionary<string, double>();

            foreach (var keyword in keywords)
            {
                // Simple weight calculation - could be enhanced with usage frequency
                weights[keyword.Trim()] = 0.8;
            }

            return weights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting keyword weights for category {CategoryId}", categoryId);
            return new Dictionary<string, double>();
        }
    }

    public async Task<List<CategorySuggestionDto>> GetTopSuggestionsAsync(string searchText, TransactionType transactionType, int maxResults = 5)
    {
        return await SuggestCategoriesAsync(searchText, null, null, transactionType);
    }

    public async Task UpdateCategoryKeywordsAsync(Guid categoryId, List<string> newKeywords)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null) return;

            category.Keywords = string.Join(",", newKeywords.Take(20));
            category.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating keywords for category {CategoryId}", categoryId);
        }
    }

    public async Task<bool> IsGermanMerchantAsync(string merchantName)
    {
        if (string.IsNullOrWhiteSpace(merchantName))
            return false;

        var merchantLower = merchantName.ToLower();
        return _germanMerchants.Values
            .SelectMany(merchants => merchants)
            .Any(merchant => merchantLower.Contains(merchant.ToLower()));
    }

    #region Helper Methods

    private string GetMatchReason(string searchText, Category category)
    {
        if (!string.IsNullOrEmpty(category.Keywords))
        {
            var keywords = category.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var matchedKeyword = keywords.FirstOrDefault(k => searchText.Contains(k.Trim().ToLower()));
            if (matchedKeyword != null)
                return $"Keyword match: {matchedKeyword}";
        }

        var categoryKey = GetCategoryKeyFromName(category.DisplayName);
        if (!string.IsNullOrEmpty(categoryKey) && _germanCategoryKeywords.ContainsKey(categoryKey))
        {
            var germanKeywords = _germanCategoryKeywords[categoryKey];
            var matchedGermanKeyword = germanKeywords.FirstOrDefault(k => searchText.Contains(k));
            if (matchedGermanKeyword != null)
                return $"German keyword: {matchedGermanKeyword}";
        }

        return "Pattern recognition";
    }

    private string GetCategoryKeyFromName(string categoryName)
    {
        var nameLower = categoryName.ToLower();
        if (nameLower.Contains("gehalt") || nameLower.Contains("lohn")) return "salary";
        if (nameLower.Contains("freelance") || nameLower.Contains("honorar")) return "freelance";
        if (nameLower.Contains("miete") || nameLower.Contains("wohnen")) return "housing";
        if (nameLower.Contains("transport") || nameLower.Contains("verkehr")) return "transportation";
        if (nameLower.Contains("essen") || nameLower.Contains("lebensmittel")) return "food";
        if (nameLower.Contains("gesundheit") || nameLower.Contains("medizin")) return "healthcare";
        if (nameLower.Contains("unterhaltung") || nameLower.Contains("freizeit")) return "entertainment";
        if (nameLower.Contains("einkauf") || nameLower.Contains("shopping")) return "shopping";
        if (nameLower.Contains("bildung") || nameLower.Contains("ausbildung")) return "education";
        if (nameLower.Contains("geschäft") || nameLower.Contains("büro")) return "business";
        
        return string.Empty;
    }

    private List<string> GetCommonMerchantsForCategory(string categoryName)
    {
        var categoryKey = GetCategoryKeyFromName(categoryName);
        if (!string.IsNullOrEmpty(categoryKey) && _germanMerchants.ContainsKey(categoryKey))
        {
            return _germanMerchants[categoryKey];
        }

        // Fallback based on category name patterns
        return categoryName.ToLower() switch
        {
            var name when name.Contains("lebensmittel") || name.Contains("essen") => 
                new() { "rewe", "edeka", "aldi", "lidl", "netto", "kaufland", "penny" },
            var name when name.Contains("transport") || name.Contains("verkehr") => 
                new() { "deutsche bahn", "db", "mvg", "shell", "aral", "esso", "uber" },
            var name when name.Contains("einkauf") || name.Contains("shopping") => 
                new() { "amazon", "zalando", "otto", "h&m", "zara", "ikea" },
            var name when name.Contains("unterhaltung") => 
                new() { "netflix", "spotify", "amazon prime", "disney+" },
            _ => new List<string>()
        };
    }

    private double GetAmountBasedScore(string categoryName, decimal amount)
    {
        // Provide small hints based on typical amounts for categories
        return categoryName.ToLower() switch
        {
            var name when name.Contains("miete") && amount > 500 => 0.1,
            var name when name.Contains("gehalt") && amount > 1000 => 0.1,
            var name when name.Contains("lebensmittel") && amount < 200 => 0.1,
            var name when name.Contains("transport") && amount < 100 => 0.1,
            _ => 0.0
        };
    }

    private bool IsCommonWord(string word)
    {
        var commonWords = new[] { "und", "der", "die", "das", "eine", "ein", "mit", "für", "von", "auf", "in", "zu", "an", "bei", "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };
        return commonWords.Contains(word.ToLower());
    }

    #endregion
}