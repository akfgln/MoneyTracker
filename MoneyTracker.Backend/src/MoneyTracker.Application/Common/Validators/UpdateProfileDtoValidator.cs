using FluentValidation;
using MoneyTracker.Application.DTOs.Auth;

namespace MoneyTracker.Application.Common.Validators;

public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Vorname ist erforderlich")
            .MaximumLength(100).WithMessage("Vorname darf maximal 100 Zeichen lang sein")
            .Matches(@"^[a-zA-ZÀ-ſ\s-']+$").WithMessage("Vorname enthält ungültige Zeichen");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Nachname ist erforderlich")
            .MaximumLength(100).WithMessage("Nachname darf maximal 100 Zeichen lang sein")
            .Matches(@"^[a-zA-ZÀ-ſ\s-']+$").WithMessage("Nachname enthält ungültige Zeichen");

        RuleFor(x => x.PreferredLanguage)
            .MaximumLength(10).WithMessage("Sprache darf maximal 10 Zeichen lang sein")
            .Matches(@"^[a-z]{2}-[A-Z]{2}$").WithMessage("Ungültiges Sprachformat (z.B. de-DE)");

        RuleFor(x => x.PreferredCurrency)
            .MaximumLength(3).WithMessage("Währung darf maximal 3 Zeichen lang sein")
            .Matches(@"^[A-Z]{3}$").WithMessage("Ungültiges Währungsformat (z.B. EUR)");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Ungültiges Telefonnummer-Format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.Date).WithMessage("Geburtsdatum muss in der Vergangenheit liegen")
            .GreaterThan(DateTime.UtcNow.Date.AddYears(-120)).WithMessage("Geburtsdatum ist nicht plausibel")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adresse darf maximal 500 Zeichen lang sein");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Stadt darf maximal 100 Zeichen lang sein");

        RuleFor(x => x.PostalCode)
            .MaximumLength(10).WithMessage("Postleitzahl darf maximal 10 Zeichen lang sein")
            .Matches(@"^\d{5}$").WithMessage("Deutsche Postleitzahl muss 5 Ziffern enthalten")
            .When(x => !string.IsNullOrEmpty(x.PostalCode) && x.Country == "Germany");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Land darf maximal 100 Zeichen lang sein");
    }
}