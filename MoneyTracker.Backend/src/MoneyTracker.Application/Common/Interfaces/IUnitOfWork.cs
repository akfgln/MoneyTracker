namespace MoneyTracker.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IAccountRepository Accounts { get; }
    ICategoryRepository Categories { get; }
    ITransactionRepository Transactions { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}