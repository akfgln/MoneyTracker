using FluentValidation;
using MoneyTracker.Application.DTOs.Transaction;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Validators;

/// <summary>
/// Validator for CreateTransactionDto with German error messages
/// </summary>
public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Der Betrag muss größer als 0 sein")
            .LessThan(1_000_000)
            .WithMessage("Der Betrag darf nicht größer als 1.000.000 EUR sein");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Beschreibung ist erforderlich")
            .MaximumLength(500)
            .WithMessage("Beschreibung darf maximal 500 Zeichen haben")
            .MinimumLength(3)
            .WithMessage("Beschreibung muss mindestens 3 Zeichen haben");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .WithMessage("Transaktionsdatum ist erforderlich")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("Transaktionsdatum kann nicht in der Zukunft liegen")
            .GreaterThan(DateTime.Today.AddYears(-10))
            .WithMessage("Transaktionsdatum kann nicht mehr als 10 Jahre in der Vergangenheit liegen");

        RuleFor(x => x.BookingDate)
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .When(x => x.BookingDate.HasValue)
            .WithMessage("Buchungsdatum kann nicht in der Zukunft liegen")
            .GreaterThan(DateTime.Today.AddYears(-10))
            .When(x => x.BookingDate.HasValue)
            .WithMessage("Buchungsdatum kann nicht mehr als 10 Jahre in der Vergangenheit liegen");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Währung ist erforderlich")
            .Must(BeValidCurrency)
            .WithMessage("Ungültige Währung. Unterstützte Währungen: EUR, USD, GBP, CHF");

        RuleFor(x => x.CustomVatRate)
            .InclusiveBetween(0m, 1m)
            .When(x => x.CustomVatRate.HasValue)
            .WithMessage("MwSt.-Satz muss zwischen 0% und 100% liegen");

        RuleFor(x => x.TransactionType)
            .IsInEnum()
            .WithMessage("Ungültiger Transaktionstyp");

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Konto ist erforderlich");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Kategorie ist erforderlich");

        RuleFor(x => x.MerchantName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.MerchantName))
            .WithMessage("Händlername darf maximal 200 Zeichen haben");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notizen dürfen maximal 1000 Zeichen haben");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.ReferenceNumber))
            .WithMessage("Referenznummer darf maximal 100 Zeichen haben");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod))
            .WithMessage("Zahlungsmethode darf maximal 50 Zeichen haben");

        RuleFor(x => x.Location)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Location))
            .WithMessage("Ort darf maximal 200 Zeichen haben");

        RuleFor(x => x.Tags)
            .Must(BeValidTags)
            .When(x => x.Tags != null && x.Tags.Any())
            .WithMessage("Tags dürfen nur Buchstaben, Zahlen und Bindestriche enthalten und maximal 30 Zeichen lang sein");

        RuleFor(x => x.RecurrencePattern)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.RecurrencePattern))
            .WithMessage("Wiederholungsmuster darf maximal 100 Zeichen haben");

        // Business rule: If IsRecurring is true, RecurrencePattern must be provided
        RuleFor(x => x.RecurrencePattern)
            .NotEmpty()
            .When(x => x.IsRecurring)
            .WithMessage("Wiederholungsmuster ist erforderlich wenn die Transaktion wiederkehrend ist");
    }

    private bool BeValidCurrency(string currency)
    {
        var supportedCurrencies = new[] { "EUR", "USD", "GBP", "CHF" };
        return supportedCurrencies.Contains(currency?.ToUpper());
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
}