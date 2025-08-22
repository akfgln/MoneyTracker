using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Category;

public class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CategoryType CategoryType { get; set; }
    public string CategoryTypeName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public decimal DefaultVatRate { get; set; }
    public string? Keywords { get; set; }
    public bool IsSystemCategory { get; set; }
    public bool IsActive { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<CategoryResponseDto> SubCategories { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}