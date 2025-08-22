using MoneyTracker.Domain.Common;
using System.Linq.Expressions;

namespace MoneyTracker.Application.Common.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    // Query operations
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    
    // Paged queries
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);
    
    // Command operations
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Soft delete operations
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Include operations for navigation properties
    IQueryable<T> Include(params Expression<Func<T, object>>[] includes);
    Task<T?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);
}