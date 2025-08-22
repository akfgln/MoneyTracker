using Microsoft.EntityFrameworkCore;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Business entities
    DbSet<User> Users { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Category> Categories { get; }
    DbSet<UploadedFile> UploadedFiles { get; }
    
    // Authentication entities
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<UserConsent> UserConsents { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}