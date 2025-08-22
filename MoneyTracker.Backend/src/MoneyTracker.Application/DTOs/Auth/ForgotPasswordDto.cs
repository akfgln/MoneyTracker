using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "E-Mail ist erforderlich")]
    [EmailAddress(ErrorMessage = "Ungültiges E-Mail-Format")]
    public string Email { get; set; } = string.Empty;
}