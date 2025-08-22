using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "E-Mail ist erforderlich")]
    [EmailAddress(ErrorMessage = "Ungültiges E-Mail-Format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passwort ist erforderlich")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}