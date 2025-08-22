using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Domain.Common;
using MoneyTracker.Domain.Entities;

namespace MoneyTracker.Infrastructure.Persistence;

public class SqlServerDbContext : IdentityDbContext<User, ApplicationRole, Guid>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public SqlServerDbContext(
        DbContextOptions<SqlServerDbContext> options,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
        : base(options)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    // DbSets for business entities
    public override DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
    
    // DbSets for authentication entities
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call the base method first for Identity configuration
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlServerDbContext).Assembly);
        
        // Configure global query filters for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Skip Identity entities for soft delete filters
            if (entityType.ClrType.Namespace?.StartsWith("Microsoft.AspNetCore.Identity") == true)
                continue;
                
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
                var isDeletedProperty = Expression.Call(propertyMethodInfo!, parameter, Expression.Constant("IsDeleted"));
                var compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
                var lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
            
            // Apply soft delete filter to User entity as well
            if (entityType.ClrType == typeof(User))
            {
                var parameter = Expression.Parameter(typeof(User), "u");
                var property = Expression.Property(parameter, nameof(User.IsDeleted));
                var compareExpression = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda<Func<User, bool>>(compareExpression, parameter);

                modelBuilder.Entity<User>().HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    entry.Entity.CreatedAt = _dateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedBy = _currentUserService.UserId;
                    entry.Entity.UpdatedAt = _dateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedBy = _currentUserService.UserId;
                    entry.Entity.DeletedAt = _dateTime.UtcNow;
                    break;
            }
        }
        
        // Handle User entity audit separately since it doesn't inherit from BaseEntity
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    entry.Entity.CreatedAt = _dateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedBy = _currentUserService.UserId;
                    entry.Entity.UpdatedAt = _dateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedBy = _currentUserService.UserId;
                    entry.Entity.DeletedAt = _dateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}