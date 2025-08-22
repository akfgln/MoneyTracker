using FluentValidation;
using MoneyTracker.Application.DTOs.Category;

namespace MoneyTracker.Application.Common.Validators;

public class BulkUpdateCategoriesDtoValidator : AbstractValidator<BulkUpdateCategoriesDto>
{
    public BulkUpdateCategoriesDtoValidator()
    {
        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage("At least one category ID is required")
            .Must(ids => ids.Count <= 100).WithMessage("Cannot update more than 100 categories at once");

        RuleForEach(x => x.CategoryIds)
            .NotEmpty().WithMessage("Category ID cannot be empty");

        RuleFor(x => x.Updates)
            .NotNull().WithMessage("Updates are required")
            .SetValidator(new UpdateCategoryDtoValidator());
    }
}