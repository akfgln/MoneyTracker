using System.Security.Claims;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
    DateTime GetTokenExpiration(string token);
    string GetUserIdFromToken(string token);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<string> GeneratePasswordResetTokenAsync(User user);
}