using FluentValidation;
using MoneyTracker.Application.DTOs.File;

namespace MoneyTracker.Application.Common.Validators;

/// <summary>
/// Validator for UploadStatementDto with German error messages
/// </summary>
public class UploadStatementDtoValidator : AbstractValidator<UploadStatementDto>
{
    public UploadStatementDtoValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("Datei ist erforderlich")
            .Must(file => file != null && file.Length > 0)
            .WithMessage("Datei darf nicht leer sein")
            .Must(file => file == null || file.Length <= 10 * 1024 * 1024)
            .WithMessage("Datei ist zu groß. Maximale Größe: 10 MB")
            .Must(file => file == null || IsValidFileType(file.FileName))
            .WithMessage("Ungültiger Dateityp. Nur PDF-Dateien sind erlaubt");

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Konto ist erforderlich");

        RuleFor(x => x.BankName)
            .NotEmpty()
            .WithMessage("Bankname ist erforderlich")
            .MaximumLength(100)
            .WithMessage("Bankname darf maximal 100 Zeichen haben")
            .Must(BeValidBankName)
            .WithMessage("Ungültiger Bankname. Nur Buchstaben, Zahlen und Leerzeichen sind erlaubt");

        RuleFor(x => x.StatementPeriodStart)
            .LessThan(x => x.StatementPeriodEnd)
            .When(x => x.StatementPeriodStart.HasValue && x.StatementPeriodEnd.HasValue)
            .WithMessage("Startdatum muss vor dem Enddatum liegen")
            .LessThanOrEqualTo(DateTime.Today)
            .When(x => x.StatementPeriodStart.HasValue)
            .WithMessage("Startdatum kann nicht in der Zukunft liegen");

        RuleFor(x => x.StatementPeriodEnd)
            .GreaterThan(x => x.StatementPeriodStart)
            .When(x => x.StatementPeriodStart.HasValue && x.StatementPeriodEnd.HasValue)
            .WithMessage("Enddatum muss nach dem Startdatum liegen")
            .LessThanOrEqualTo(DateTime.Today)
            .When(x => x.StatementPeriodEnd.HasValue)
            .WithMessage("Enddatum kann nicht in der Zukunft liegen");

        // Business rule: If period is specified, it should be reasonable (not more than 1 year)
        RuleFor(x => x)
            .Must(dto => !dto.StatementPeriodStart.HasValue || !dto.StatementPeriodEnd.HasValue ||
                        (dto.StatementPeriodEnd.Value - dto.StatementPeriodStart.Value).TotalDays <= 366)
            .WithMessage("Auszugszeitraum darf nicht länger als ein Jahr sein");
    }

    private bool IsValidFileType(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLower();
        return extension == ".pdf";
    }

    private bool BeValidBankName(string bankName)
    {
        if (string.IsNullOrWhiteSpace(bankName))
            return false;

        // Allow letters, numbers, spaces, and common bank name characters
        return bankName.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '&' || c == '-' || c == '.');
    }
}