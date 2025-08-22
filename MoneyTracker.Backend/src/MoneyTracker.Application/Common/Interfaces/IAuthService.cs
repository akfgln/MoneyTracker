using MoneyTracker.Application.DTOs.Auth;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto request, string? ipAddress = null);
    Task<AuthResponseDto> LoginAsync(LoginDto request, string? ipAddress = null);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordDto request);
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
    Task<UserProfileDto?> UpdateUserProfileAsync(Guid userId, UpdateProfileDto request);
    Task<bool> DeleteUserAccountAsync(Guid userId);
    Task<UserConsentDto?> GetUserConsentAsync(Guid userId);
    Task<bool> UpdateUserConsentAsync(Guid userId, UserConsentDto consent, string? ipAddress = null);
}