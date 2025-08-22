using Microsoft.AspNetCore.Identity;
using MoneyTracker.Domain.Common;

namespace MoneyTracker.Domain.Entities;

public class User : IdentityUser<Guid>
{
    // Additional properties beyond IdentityUser
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? LastLoginDate { get; set; }
    public string PreferredLanguage { get; set; } = "de-DE";
    public string PreferredCurrency { get; set; } = "EUR";
    public string? ProfileImagePath { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; } = "Germany";
    public bool AcceptedTerms { get; set; } = false;
    public DateTime? TermsAcceptedDate { get; set; }
    public bool AcceptedPrivacyPolicy { get; set; } = false;
    public DateTime? PrivacyPolicyAcceptedDate { get; set; }
    public string? EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmationTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedDate { get; set; }
    public string? DeactivationReason { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual UserConsent? UserConsent { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string DisplayName => string.IsNullOrWhiteSpace(FullName) ? Email ?? "Unknown" : FullName;
    public bool IsGermanResident => Country?.Equals("Germany", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsMinor => DateOfBirth.HasValue && DateTime.UtcNow.Date.AddYears(-18) < DateOfBirth.Value.Date;
    public bool HasValidEmailConfirmationToken => !string.IsNullOrEmpty(EmailConfirmationToken) && 
        EmailConfirmationTokenExpiry.HasValue && EmailConfirmationTokenExpiry.Value > DateTime.UtcNow;
    public bool HasValidPasswordResetToken => !string.IsNullOrEmpty(PasswordResetToken) && 
        PasswordResetTokenExpiry.HasValue && PasswordResetTokenExpiry.Value > DateTime.UtcNow;
    
    public User()
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    // Helper methods
    public bool IsPasswordResetTokenValid()
    {
        return !string.IsNullOrEmpty(PasswordResetToken) && 
               PasswordResetTokenExpiry.HasValue && 
               PasswordResetTokenExpiry.Value > DateTime.UtcNow;
    }

    public bool IsEmailConfirmationTokenValid()
    {
        return !string.IsNullOrEmpty(EmailConfirmationToken) && 
               EmailConfirmationTokenExpiry.HasValue && 
               EmailConfirmationTokenExpiry.Value > DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        EmailConfirmationToken = null;
        EmailConfirmationTokenExpiry = null;
    }

    public void UpdateLastLogin()
    {
        LastLoginDate = DateTime.UtcNow;
    }

    public void AcceptTermsAndPrivacyPolicy()
    {
        AcceptedTerms = true;
        TermsAcceptedDate = DateTime.UtcNow;
        AcceptedPrivacyPolicy = true;
        PrivacyPolicyAcceptedDate = DateTime.UtcNow;
    }

    public void Deactivate(string reason)
    {
        IsActive = false;
        DeactivatedDate = DateTime.UtcNow;
        DeactivationReason = reason;
    }

    public void Reactivate()
    {
        IsActive = true;
        DeactivatedDate = null;
        DeactivationReason = null;
    }
}