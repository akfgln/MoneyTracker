namespace MoneyTracker.Application.DTOs.Category;

public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public decimal? DefaultVatRate { get; set; }
    public string? Keywords { get; set; }
    public bool? IsActive { get; set; }
}