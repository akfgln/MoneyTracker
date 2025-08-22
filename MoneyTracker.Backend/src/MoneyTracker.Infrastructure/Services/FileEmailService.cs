using MoneyTracker.Application.Common.Interfaces;

namespace MoneyTracker.Infrastructure.Services;

public class FileEmailService : IEmailService
{
    private readonly string _outputDirectory;

    public FileEmailService()
    {
        _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "emails");
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    public async Task SendEmailConfirmationAsync(string email, string firstName, string token)
    {
        var subject = "E-Mail-Adresse bestätigen - MoneyTracker";
        var body = $@"
Hallo {firstName},

vielen Dank für Ihre Registrierung bei MoneyTracker!

Bitte bestätigen Sie Ihre E-Mail-Adresse, indem Sie den folgenden Token verwenden:

Token: {token}

Oder verwenden Sie diesen Link:
https://localhost:5001/api/auth/confirm-email?userId={email}&token={token}

Dieser Token ist 24 Stunden gültig.

Vielen Dank,
Ihr MoneyTracker Team
";

        await SaveEmailToFileAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string firstName, string token)
    {
        var subject = "Passwort zurücksetzen - MoneyTracker";
        var body = $@"
Hallo {firstName},

Sie haben eine Passwort-Zurücksetzung für Ihr MoneyTracker-Konto angefordert.

Verwenden Sie den folgenden Token, um Ihr Passwort zurückzusetzen:

Token: {token}

Oder verwenden Sie diesen Link:
https://localhost:5001/api/auth/reset-password?email={email}&token={token}

Dieser Token ist 1 Stunde gültig.

Wenn Sie diese Anfrage nicht gestellt haben, ignorieren Sie diese E-Mail.

Vielen Dank,
Ihr MoneyTracker Team
";

        await SaveEmailToFileAsync(email, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        var subject = "Willkommen bei MoneyTracker!";
        var body = $@"
Hallo {firstName},

herzlich willkommen bei MoneyTracker!

Ihre E-Mail-Adresse wurde erfolgreich bestätigt und Ihr Konto ist jetzt aktiv.

Sie können sich jetzt anmelden und mit der Verwaltung Ihrer Finanzen beginnen.

Bei Fragen stehen wir Ihnen gerne zur Verfügung.

Vielen Dank,
Ihr MoneyTracker Team
";

        await SaveEmailToFileAsync(email, subject, body);
    }

    public async Task SendAccountDeactivationAsync(string email, string firstName)
    {
        var subject = "Konto deaktiviert - MoneyTracker";
        var body = $@"
Hallo {firstName},

Ihr MoneyTracker-Konto wurde erfolgreich deaktiviert.

Alle Ihre Daten wurden entsprechend der DSGVO-Bestimmungen verarbeitet.

Wenn Sie Fragen haben oder Ihr Konto reaktivieren möchten, wenden Sie sich bitte an unseren Support.

Vielen Dank,
Ihr MoneyTracker Team
";

        await SaveEmailToFileAsync(email, subject, body);
    }

    public async Task SendSecurityAlertAsync(string email, string firstName, string alertType, string details)
    {
        var subject = $"Sicherheitswarnung - {alertType} - MoneyTracker";
        var body = $@"
Hallo {firstName},

wir haben eine verdächtige Aktivität in Ihrem MoneyTracker-Konto festgestellt.

Art der Warnung: {alertType}
Details: {details}
Zeitpunkt: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

Wenn Sie diese Aktivität nicht autorisiert haben, ändern Sie bitte sofort Ihr Passwort und wenden Sie sich an unseren Support.

Vielen Dank,
Ihr MoneyTracker Team
";

        await SaveEmailToFileAsync(email, subject, body);
    }

    private async Task SaveEmailToFileAsync(string email, string subject, string body)
    {
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{email.Replace("@", "_")}.txt";
        var filePath = Path.Combine(_outputDirectory, fileName);
        
        var content = $@"TO: {email}
SUBJECT: {subject}
DATE: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

{body}";
        
        await File.WriteAllTextAsync(filePath, content);
    }
}