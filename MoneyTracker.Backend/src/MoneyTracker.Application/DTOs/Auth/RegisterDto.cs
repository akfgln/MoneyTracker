using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Email ist erforderlich")]
    [EmailAddress(ErrorMessage = "Ungültiges E-Mail-Format")]
    [MaxLength(320, ErrorMessage = "E-Mail darf maximal 320 Zeichen lang sein")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vorname ist erforderlich")]
    [MaxLength(100, ErrorMessage = "Vorname darf maximal 100 Zeichen lang sein")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nachname ist erforderlich")]
    [MaxLength(100, ErrorMessage = "Nachname darf maximal 100 Zeichen lang sein")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passwort ist erforderlich")]
    [MinLength(12, ErrorMessage = "Passwort muss mindestens 12 Zeichen lang sein")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$", 
        ErrorMessage = "Passwort muss mindestens 12 Zeichen enthalten, darunter Groß- und Kleinbuchstaben, Ziffern und Sonderzeichen")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passwort-Bestätigung ist erforderlich")]
    [Compare(nameof(Password), ErrorMessage = "Passwörter stimmen nicht überein")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [MaxLength(10)]
    public string PreferredLanguage { get; set; } = "de-DE";

    [Required(ErrorMessage = "Sie müssen die Datenschutzrichtlinie akzeptieren")]
    public bool AcceptPrivacyPolicy { get; set; }

    [Required(ErrorMessage = "Sie müssen die Nutzungsbedingungen akzeptieren")]
    public bool AcceptTermsOfService { get; set; }

    public bool MarketingEmailsConsent { get; set; } = false;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Country { get; set; } = "Germany";
}