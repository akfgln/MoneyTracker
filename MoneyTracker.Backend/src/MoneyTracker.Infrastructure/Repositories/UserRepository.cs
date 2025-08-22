using Microsoft.EntityFrameworkCore;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Infrastructure.Repositories;

/// <summary>
/// Custom repository for User-specific operations.
/// Standard CRUD operations should use ASP.NET Core Identity UserManager.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DbContext _context;
    private readonly DbSet<User> _users;

    public UserRepository(DbContext context)
    {
        _context = context;
        _users = context.Set<User>();
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _users.FirstOrDefaultAsync(
            u => u.EmailConfirmationToken == token && 
                 u.EmailConfirmationTokenExpiry.HasValue && 
                 u.EmailConfirmationTokenExpiry.Value > DateTime.UtcNow, 
            cancellationToken);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _users.FirstOrDefaultAsync(
            u => u.PasswordResetToken == token && 
                 u.PasswordResetTokenExpiry.HasValue && 
                 u.PasswordResetTokenExpiry.Value > DateTime.UtcNow, 
            cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _users.Where(u => u.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await _users.Where(u => u.Country == country).ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalActiveUsersCountAsync(CancellationToken cancellationToken = default)
    {
        return await _users.CountAsync(u => u.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetRecentlyRegisteredUsersAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _users
            .Where(u => u.CreatedAt >= cutoffDate)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}