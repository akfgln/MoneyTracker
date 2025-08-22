using FluentValidation;
using MoneyTracker.Application.DTOs.Category;

namespace MoneyTracker.Application.Common.Validators;

public class ImportCategoriesDtoValidator : AbstractValidator<ImportCategoriesDto>
{
    public ImportCategoriesDtoValidator()
    {
        RuleFor(x => x.Categories)
            .NotEmpty().WithMessage("At least one category is required for import")
            .Must(categories => categories.Count <= 1000).WithMessage("Cannot import more than 1000 categories at once");

        RuleForEach(x => x.Categories)
            .SetValidator(new ImportCategoryItemDtoValidator());
    }
}

public class ImportCategoryItemDtoValidator : AbstractValidator<ImportCategoryItemDto>
{
    public ImportCategoryItemDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(200).WithMessage("Category name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.CategoryType)
            .IsInEnum().WithMessage("Invalid category type");

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon must not exceed 50 characters");

        RuleFor(x => x.Color)
            .MaximumLength(10).WithMessage("Color must not exceed 10 characters")
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .When(x => !string.IsNullOrEmpty(x.Color))
            .WithMessage("Color must be a valid hex color code");

        RuleFor(x => x.ParentCategoryName)
            .MaximumLength(200).WithMessage("Parent category name must not exceed 200 characters");

        RuleFor(x => x.DefaultVatRate)
            .GreaterThanOrEqualTo(0).WithMessage("VAT rate cannot be negative")
            .LessThanOrEqualTo(1).WithMessage("VAT rate cannot exceed 100%");

        RuleFor(x => x.Keywords)
            .MaximumLength(1000).WithMessage("Keywords must not exceed 1000 characters");
    }
}