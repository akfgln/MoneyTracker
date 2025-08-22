using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs.Auth;

public class ConfirmEmailDto
{
    [Required(ErrorMessage = "Benutzer-ID ist erforderlich")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bestätigungs-Token ist erforderlich")]
    public string Token { get; set; } = string.Empty;
}