namespace MoneyTracker.Application.DTOs.Auth;

public class UserConsentDto
{
    public bool PrivacyPolicyAccepted { get; set; }
    public DateTime? PrivacyPolicyAcceptedDate { get; set; }
    public string PrivacyPolicyVersion { get; set; } = "1.0";
    public bool TermsOfServiceAccepted { get; set; }
    public DateTime? TermsOfServiceAcceptedDate { get; set; }
    public string TermsOfServiceVersion { get; set; } = "1.0";
    public bool MarketingEmailsConsent { get; set; }
    public bool DataProcessingConsent { get; set; }
    public bool CookieConsent { get; set; }
}