using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Category;

public class CategoryHierarchyDto
{
    public CategoryType CategoryType { get; set; }
    public string CategoryTypeName { get; set; } = string.Empty;
    public List<CategoryResponseDto> Categories { get; set; } = new();
}

public class SuggestCategoryDto
{
    public string Description { get; set; } = string.Empty;
    public string? MerchantName { get; set; }
    public decimal? Amount { get; set; }
    public TransactionType TransactionType { get; set; }
}

public class CategorySuggestionDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public double ConfidenceScore { get; set; }
    public string MatchReason { get; set; } = string.Empty;
}