using FluentValidation;
using MoneyTracker.Application.DTOs.Auth;

namespace MoneyTracker.Application.Common.Validators;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-Mail ist erforderlich")
            .EmailAddress().WithMessage("Ungültiges E-Mail-Format");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token ist erforderlich");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Neues Passwort ist erforderlich")
            .MinimumLength(12).WithMessage("Passwort muss mindestens 12 Zeichen lang sein")
            .Matches(@"^(?=.*[a-z])").WithMessage("Passwort muss mindestens einen Kleinbuchstaben enthalten")
            .Matches(@"^(?=.*[A-Z])").WithMessage("Passwort muss mindestens einen Großbuchstaben enthalten")
            .Matches(@"^(?=.*\d)").WithMessage("Passwort muss mindestens eine Ziffer enthalten")
            .Matches(@"^(?=.*[^\da-zA-Z])").WithMessage("Passwort muss mindestens ein Sonderzeichen enthalten")
            .Must(password => password.Distinct().Count() >= 6).WithMessage("Passwort muss mindestens 6 verschiedene Zeichen enthalten");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Passwort-Bestätigung ist erforderlich")
            .Equal(x => x.NewPassword).WithMessage("Passwörter stimmen nicht überein");
    }
}