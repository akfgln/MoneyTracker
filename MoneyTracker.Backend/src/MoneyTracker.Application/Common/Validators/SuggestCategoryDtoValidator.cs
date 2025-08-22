using FluentValidation;
using MoneyTracker.Application.DTOs.Category;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Validators;

public class SuggestCategoryDtoValidator : AbstractValidator<SuggestCategoryDto>
{
    public SuggestCategoryDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required for category suggestion")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.MerchantName)
            .MaximumLength(200).WithMessage("Merchant name must not exceed 200 characters")
            .When(x => x.MerchantName != null);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Invalid transaction type");
    }
}