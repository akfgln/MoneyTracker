using MoneyTracker.Application.DTOs.Auth;

namespace MoneyTracker.Application.Common.Interfaces;

public interface IGdprService
{
    Task<byte[]> ExportUserDataAsync(Guid userId);
    Task<bool> DeleteUserDataAsync(Guid userId);
    Task<UserConsentDto?> GetUserConsentAsync(Guid userId);
    Task<bool> UpdateUserConsentAsync(Guid userId, UserConsentDto consent, string? ipAddress = null);
    Task<bool> ProcessDataDeletionRequestAsync(Guid userId, string reason);
    Task<string> GenerateDataExportReportAsync(Guid userId);
}