using FluentValidation;
using MoneyTracker.Application.DTOs.Category;

namespace MoneyTracker.Application.Common.Validators;

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required when provided")
            .MaximumLength(200).WithMessage("Category name must not exceed 200 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon must not exceed 50 characters")
            .When(x => x.Icon != null);

        RuleFor(x => x.Color)
            .MaximumLength(10).WithMessage("Color must not exceed 10 characters")
            .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
            .When(x => !string.IsNullOrEmpty(x.Color))
            .WithMessage("Color must be a valid hex color code (e.g., #FF0000 or #F00)");

        RuleFor(x => x.DefaultVatRate)
            .GreaterThanOrEqualTo(0).WithMessage("VAT rate cannot be negative")
            .LessThanOrEqualTo(1).WithMessage("VAT rate cannot exceed 100%")
            .When(x => x.DefaultVatRate.HasValue);

        RuleFor(x => x.Keywords)
            .MaximumLength(1000).WithMessage("Keywords must not exceed 1000 characters")
            .When(x => x.Keywords != null);
    }
}