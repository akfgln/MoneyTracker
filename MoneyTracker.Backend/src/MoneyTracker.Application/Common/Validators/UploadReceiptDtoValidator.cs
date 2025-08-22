using FluentValidation;
using MoneyTracker.Application.DTOs.File;

namespace MoneyTracker.Application.Common.Validators;

/// <summary>
/// Validator for UploadReceiptDto with German error messages
/// </summary>
public class UploadReceiptDtoValidator : AbstractValidator<UploadReceiptDto>
{
    public UploadReceiptDtoValidator()
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

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Beschreibung darf maximal 500 Zeichen haben");

        RuleFor(x => x.Tags)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Tags))
            .WithMessage("Tags dürfen maximal 200 Zeichen haben")
            .Must(BeValidTags)
            .When(x => !string.IsNullOrEmpty(x.Tags))
            .WithMessage("Tags müssen durch Kommas getrennt sein und dürfen keine Sonderzeichen enthalten");
    }

    private bool IsValidFileType(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLower();
        return extension == ".pdf";
    }

    private bool BeValidTags(string tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return true;

        var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();

        return tagList.All(tag =>
            !string.IsNullOrWhiteSpace(tag) &&
            tag.Length <= 30 &&
            tag.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' ')
        );
    }
}