using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.DTOs.Category;

public class BulkUpdateCategoriesDto
{
    public List<Guid> CategoryIds { get; set; } = new();
    public UpdateCategoryDto Updates { get; set; } = new();
}

public class MergeCategoryDto
{
    public Guid TargetCategoryId { get; set; }
    public bool DeleteSourceCategory { get; set; } = true;
}

public class ImportCategoriesDto
{
    public List<ImportCategoryItemDto> Categories { get; set; } = new();
    public bool OverwriteExisting { get; set; } = false;
}

public class ImportCategoryItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CategoryType CategoryType { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? ParentCategoryName { get; set; }
    public decimal DefaultVatRate { get; set; } = 0.19m;
    public string? Keywords { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CategoryQueryParameters
{
    public CategoryType? CategoryType { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsSystemCategory { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}