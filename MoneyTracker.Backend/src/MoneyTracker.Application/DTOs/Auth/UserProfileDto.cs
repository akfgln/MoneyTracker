namespace MoneyTracker.Application.DTOs.Auth;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "de-DE";
    public string PreferredCurrency { get; set; } = "EUR";
    public DateTime? LastLoginDate { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string? ProfileImagePath { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public bool IsActive { get; set; }
}