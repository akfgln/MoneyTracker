using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs.Auth;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "E-Mail ist erforderlich")]
    [EmailAddress(ErrorMessage = "Ungültiges E-Mail-Format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Token ist erforderlich")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Neues Passwort ist erforderlich")]
    [MinLength(12, ErrorMessage = "Passwort muss mindestens 12 Zeichen lang sein")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$", 
        ErrorMessage = "Passwort muss mindestens 12 Zeichen enthalten, darunter Groß- und Kleinbuchstaben, Ziffern und Sonderzeichen")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passwort-Bestätigung ist erforderlich")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwörter stimmen nicht überein")]
    public string ConfirmPassword { get; set; } = string.Empty;
}