using FluentValidation;
using MoneyTracker.Application.DTOs.Auth;

namespace MoneyTracker.Application.Common.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-Mail ist erforderlich")
            .EmailAddress().WithMessage("Ungültiges E-Mail-Format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Passwort ist erforderlich");
    }
}