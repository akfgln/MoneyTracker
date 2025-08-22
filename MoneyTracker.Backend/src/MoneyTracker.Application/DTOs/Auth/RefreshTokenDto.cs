using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs.Auth;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "Refresh Token ist erforderlich")]
    public string RefreshToken { get; set; } = string.Empty;
}