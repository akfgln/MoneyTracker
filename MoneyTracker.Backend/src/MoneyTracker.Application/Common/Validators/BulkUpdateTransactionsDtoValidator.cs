using FluentValidation;
using MoneyTracker.Application.DTOs.Transaction;

namespace MoneyTracker.Application.Common.Validators;

/// <summary>
/// Validator for BulkUpdateTransactionsDto with German error messages
/// </summary>
public class BulkUpdateTransactionsDtoValidator : AbstractValidator<BulkUpdateTransactionsDto>
{
    public BulkUpdateTransactionsDtoValidator()
    {
        RuleFor(x => x.TransactionIds)
            .NotEmpty()
            .WithMessage("Mindestens eine Transaktion muss ausgewählt werden")
            .Must(x => x.Count <= 100)
            .WithMessage("Maximal 100 Transaktionen können gleichzeitig aktualisiert werden");

        RuleFor(x => x.NewCategoryId)
            .NotEmpty()
            .When(x => x.NewCategoryId.HasValue)
            .WithMessage("Ungültige Kategorie-ID");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notizen dürfen maximal 1000 Zeichen haben");

        RuleFor(x => x.AddTags)
            .Must(BeValidTags)
            .When(x => x.AddTags != null && x.AddTags.Any())
            .WithMessage("Tags dürfen nur Buchstaben, Zahlen und Bindestriche enthalten und maximal 30 Zeichen lang sein");

        RuleFor(x => x.RemoveTags)
            .Must(BeValidTags)
            .When(x => x.RemoveTags != null && x.RemoveTags.Any())
            .WithMessage("Tags dürfen nur Buchstaben, Zahlen und Bindestriche enthalten und maximal 30 Zeichen lang sein");

        // At least one update operation must be specified
        RuleFor(x => x)
            .Must(HaveAtLeastOneUpdate)
            .WithMessage("Mindestens eine Änderung muss angegeben werden (Kategorie, Verifizierung, Verarbeitung, Tags oder Notizen)");
    }

    private bool BeValidTags(List<string> tags)
    {
        if (tags == null || !tags.Any())
            return true;

        // Check if tags are valid (alphanumeric + hyphens, max 30 chars each)
        return tags.All(tag => 
            !string.IsNullOrWhiteSpace(tag) && 
            tag.Length <= 30 && 
            tag.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
    }

    private bool HaveAtLeastOneUpdate(BulkUpdateTransactionsDto dto)
    {
        return dto.NewCategoryId.HasValue ||
               dto.MarkAsVerified.HasValue ||
               dto.MarkAsProcessed.HasValue ||
               (dto.AddTags?.Any() == true) ||
               (dto.RemoveTags?.Any() == true) ||
               !string.IsNullOrEmpty(dto.Notes);
    }
}