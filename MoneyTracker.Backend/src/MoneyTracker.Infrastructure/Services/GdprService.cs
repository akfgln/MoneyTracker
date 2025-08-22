using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Application.DTOs.Auth;
using MoneyTracker.Domain.Entities;
using System.Text;
using System.Text.Json;

namespace MoneyTracker.Infrastructure.Services;

public class GdprService : IGdprService
{
    private readonly ILogger<GdprService> _logger;
    private readonly IApplicationDbContext _context;

    public GdprService(ILogger<GdprService> logger, IApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<byte[]> ExportUserDataAsync(Guid userId)
    {
        try
        {
            // Retrieve user data from all relevant tables
            var user = await _context.Users
                .Include(u => u.Accounts)
                .Include(u => u.Transactions)
                .Include(u => u.RefreshTokens)
                .Include(u => u.UserConsent)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Array.Empty<byte>();

            // Create export data structure
            var exportData = new
            {
                PersonalData = new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.DateOfBirth,
                    user.Address,
                    user.City,
                    user.PostalCode,
                    user.Country,
                    user.PreferredLanguage,
                    user.PreferredCurrency,
                    user.CreatedAt,
                    user.LastLoginDate
                },
                Accounts = user.Accounts.Select(a => new
                {
                    a.Id,
                    a.AccountName,
                    a.AccountType,
                    a.Currency,
                    a.CurrentBalance,
                    a.CreatedAt
                }).ToList(),
                Transactions = user.Transactions.Select(t => new
                {
                    t.Id,
                    t.Amount,
                    t.Description,
                    t.TransactionDate,
                    t.TransactionType,
                    t.CreatedAt
                }).ToList(),
                ConsentData = user.UserConsent != null ? new
                {
                    user.UserConsent.PrivacyPolicyAccepted,
                    user.UserConsent.PrivacyPolicyAcceptedDate,
                    user.UserConsent.PrivacyPolicyVersion,
                    user.UserConsent.TermsOfServiceAccepted,
                    user.UserConsent.TermsOfServiceAcceptedDate,
                    user.UserConsent.TermsOfServiceVersion,
                    user.UserConsent.MarketingEmailsConsent,
                    user.UserConsent.DataProcessingConsent,
                    user.UserConsent.CookieConsent
                } : null,
                ExportInfo = new
                {
                    ExportDate = DateTime.UtcNow,
                    ExportReason = "GDPR Data Portability Request",
                    DataController = "MoneyTracker GmbH",
                    ContactEmail = "privacy@moneytracker.de"
                }
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);

            _logger.LogInformation("User data exported for user: {UserId}", userId);
            return bytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user data for user: {UserId}", userId);
            return Array.Empty<byte>();
        }
    }

    public async Task<bool> DeleteUserDataAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.RefreshTokens)
                .Include(u => u.UserConsent)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User not found for deletion: {UserId}", userId);
                return false;
            }

            // Delete related data in correct order (respecting foreign keys)
            
            // 1. Delete transactions first
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();
            _context.Transactions.RemoveRange(transactions);

            // 2. Delete accounts
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();
            _context.Accounts.RemoveRange(accounts);

            // 3. Delete refresh tokens
            var refreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync();
            _context.RefreshTokens.RemoveRange(refreshTokens);

            // 4. Delete user consent
            var userConsent = await _context.UserConsents
                .Where(uc => uc.UserId == userId)
                .FirstOrDefaultAsync();
            if (userConsent != null)
            {
                _context.UserConsents.Remove(userConsent);
            }

            // 5. Finally, delete the user
            _context.Users.Remove(user);

            // Save all changes in a single transaction
            await _context.SaveChangesAsync();

            _logger.LogInformation("User data successfully deleted (GDPR): {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user data for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<UserConsentDto?> GetUserConsentAsync(Guid userId)
    {
        try
        {
            var userConsent = await _context.UserConsents
                .FirstOrDefaultAsync(uc => uc.UserId == userId);

            if (userConsent == null)
                return null;

            return new UserConsentDto
            {
                PrivacyPolicyAccepted = userConsent.PrivacyPolicyAccepted,
                PrivacyPolicyAcceptedDate = userConsent.PrivacyPolicyAcceptedDate,
                TermsOfServiceAccepted = userConsent.TermsOfServiceAccepted,
                TermsOfServiceAcceptedDate = userConsent.TermsOfServiceAcceptedDate,
                MarketingEmailsConsent = userConsent.MarketingEmailsConsent,
                DataProcessingConsent = userConsent.DataProcessingConsent,
                CookieConsent = userConsent.CookieConsent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user consent for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> UpdateUserConsentAsync(Guid userId, UserConsentDto consent, string? ipAddress = null)
    {
        try
        {
            var existingConsent = await _context.UserConsents
                .FirstOrDefaultAsync(uc => uc.UserId == userId);

            if (existingConsent == null)
            {
                // Create new consent record
                existingConsent = new UserConsent
                {
                    UserId = userId,
                    ConsentIpAddress = ipAddress,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserConsents.Add(existingConsent);
            }

            // Update consent properties
            existingConsent.PrivacyPolicyAccepted = consent.PrivacyPolicyAccepted;
            existingConsent.PrivacyPolicyAcceptedDate = consent.PrivacyPolicyAcceptedDate;
            existingConsent.TermsOfServiceAccepted = consent.TermsOfServiceAccepted;
            existingConsent.TermsOfServiceAcceptedDate = consent.TermsOfServiceAcceptedDate;
            existingConsent.MarketingEmailsConsent = consent.MarketingEmailsConsent;
            existingConsent.DataProcessingConsent = consent.DataProcessingConsent;
            existingConsent.CookieConsent = consent.CookieConsent;
            existingConsent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User consent updated for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user consent for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ProcessDataDeletionRequestAsync(Guid userId, string reason)
    {
        try
        {
            // Log the deletion request for audit purposes
            _logger.LogInformation("Data deletion request initiated for user: {UserId}, Reason: {Reason}", userId, reason);
            
            // In a real implementation, you might want to:
            // 1. Create an audit record
            // 2. Send notification emails
            // 3. Schedule the deletion for a later time (grace period)
            // 4. Anonymize data instead of hard delete in some cases
            
            return await DeleteUserDataAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing data deletion request for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<string> GenerateDataExportReportAsync(Guid userId)
    {
        try
        {
            var exportData = await ExportUserDataAsync(userId);
            if (exportData.Length == 0)
            {
                return "No data found for the specified user.";
            }

            // Simplified report generation without dynamic JSON parsing
            var report = $@"
DSGVO Datenexport-Bericht
==========================
Benutzer-ID: {userId}
Exportdatum: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
Dateigröße: {exportData.Length} bytes

Dieser Export wurde gemäß Artikel 20 der DSGVO (Recht auf Datenübertragbarkeit) erstellt.
Alle persönlichen Daten, Kontoinformationen, Transaktionen und Einverständniserklärungen sind enthalten.
";

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating data export report for user: {UserId}", userId);
            return "Error generating export report.";
        }
    }
}