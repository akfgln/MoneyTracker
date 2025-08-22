using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Category;

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CategoryType CategoryType { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public decimal DefaultVatRate { get; set; } = 0.19m;
    public string? Keywords { get; set; } // Comma-separated for auto-categorization
    public bool IsActive { get; set; } = true;
}