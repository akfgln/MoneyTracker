using MoneyTracker.Domain.Common;

namespace MoneyTracker.Domain.Entities;

public class UserConsent : BaseEntity
{
    public Guid UserId { get; set; }
    public bool PrivacyPolicyAccepted { get; set; }
    public DateTime? PrivacyPolicyAcceptedDate { get; set; }
    public string PrivacyPolicyVersion { get; set; } = "1.0";
    public bool TermsOfServiceAccepted { get; set; }
    public DateTime? TermsOfServiceAcceptedDate { get; set; }
    public string TermsOfServiceVersion { get; set; } = "1.0";
    public bool MarketingEmailsConsent { get; set; }
    public bool DataProcessingConsent { get; set; }
    public bool CookieConsent { get; set; }
    public string? ConsentIpAddress { get; set; }
    public string? ConsentUserAgent { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}