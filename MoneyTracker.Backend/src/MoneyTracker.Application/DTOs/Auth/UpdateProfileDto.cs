using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Application.DTOs.Auth;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Vorname ist erforderlich")]
    [MaxLength(100, ErrorMessage = "Vorname darf maximal 100 Zeichen lang sein")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nachname ist erforderlich")]
    [MaxLength(100, ErrorMessage = "Nachname darf maximal 100 Zeichen lang sein")]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string PreferredLanguage { get; set; } = "de-DE";

    [MaxLength(3)]
    public string PreferredCurrency { get; set; } = "EUR";

    [Phone(ErrorMessage = "Ungültiges Telefonnummer-Format")]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}