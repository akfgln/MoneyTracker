using MoneyTracker.Domain.Entities;
using MoneyTracker.Domain.Enums;

namespace MoneyTracker.Application.Common.Interfaces;

/// <summary>
/// Custom repository interface for User entity-specific operations.
/// Standard CRUD operations are handled by ASP.NET Core Identity UserManager.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByCountryAsync(string country, CancellationToken cancellationToken = default);
    Task<int> GetTotalActiveUsersCountAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetRecentlyRegisteredUsersAsync(int days = 30, CancellationToken cancellationToken = default);
}