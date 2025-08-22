using FluentValidation;
using MoneyTracker.Application.DTOs.Auth;

namespace MoneyTracker.Application.Common.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-Mail ist erforderlich")
            .EmailAddress().WithMessage("Ungültiges E-Mail-Format")
            .MaximumLength(320).WithMessage("E-Mail darf maximal 320 Zeichen lang sein");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Vorname ist erforderlich")
            .MaximumLength(100).WithMessage("Vorname darf maximal 100 Zeichen lang sein")
            .Matches(@"^[a-zA-ZÀ-ſ\s-']+$").WithMessage("Vorname enthält ungültige Zeichen");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Nachname ist erforderlich")
            .MaximumLength(100).WithMessage("Nachname darf maximal 100 Zeichen lang sein")
            .Matches(@"^[a-zA-ZÀ-ſ\s-']+$").WithMessage("Nachname enthält ungültige Zeichen");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Passwort ist erforderlich")
            .MinimumLength(12).WithMessage("Passwort muss mindestens 12 Zeichen lang sein")
            .Matches(@"^(?=.*[a-z])").WithMessage("Passwort muss mindestens einen Kleinbuchstaben enthalten")
            .Matches(@"^(?=.*[A-Z])").WithMessage("Passwort muss mindestens einen Großbuchstaben enthalten")
            .Matches(@"^(?=.*\d)").WithMessage("Passwort muss mindestens eine Ziffer enthalten")
            .Matches(@"^(?=.*[^\da-zA-Z])").WithMessage("Passwort muss mindestens ein Sonderzeichen enthalten")
            .Must(password => password.Distinct().Count() >= 6).WithMessage("Passwort muss mindestens 6 verschiedene Zeichen enthalten");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Passwort-Bestätigung ist erforderlich")
            .Equal(x => x.Password).WithMessage("Passwörter stimmen nicht überein");

        RuleFor(x => x.AcceptPrivacyPolicy)
            .Equal(true).WithMessage("Sie müssen die Datenschutzrichtlinie akzeptieren");

        RuleFor(x => x.AcceptTermsOfService)
            .Equal(true).WithMessage("Sie müssen die Nutzungsbedingungen akzeptieren");

        RuleFor(x => x.PreferredLanguage)
            .MaximumLength(10).WithMessage("Sprache darf maximal 10 Zeichen lang sein")
            .Matches(@"^[a-z]{2}-[A-Z]{2}$").WithMessage("Ungültiges Sprachformat (z.B. de-DE)");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.Date).WithMessage("Geburtsdatum muss in der Vergangenheit liegen")
            .GreaterThan(DateTime.UtcNow.Date.AddYears(-120)).WithMessage("Geburtsdatum ist nicht plausibel");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Ungültiges Telefonnummer-Format").When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}