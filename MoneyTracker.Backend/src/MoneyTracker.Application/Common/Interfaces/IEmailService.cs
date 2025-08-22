namespace MoneyTracker.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string firstName, string token);
    Task SendPasswordResetAsync(string email, string firstName, string token);
    Task SendWelcomeEmailAsync(string email, string firstName);
    Task SendAccountDeactivationAsync(string email, string firstName);
    Task SendSecurityAlertAsync(string email, string firstName, string alertType, string details);
}