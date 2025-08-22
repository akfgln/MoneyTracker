using Microsoft.EntityFrameworkCore;
using MoneyTracker.Application.Common.Interfaces;
using MoneyTracker.Domain.Common;
using System.Linq.Expressions;

namespace MoneyTracker.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // Query operations
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null 
            ? await _dbSet.CountAsync(cancellationToken) 
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    // Paged queries
    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (predicate != null)
            query = query.Where(predicate);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        if (orderBy != null)
        {
            query = ascending 
                ? query.OrderBy(orderBy) 
                : query.OrderByDescending(orderBy);
        }
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (items, totalCount);
    }

    // Command operations
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    // Soft delete operations
    public virtual async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await SoftDeleteAsync(entity, cancellationToken);
        }
    }

    public virtual async Task SoftDeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        
        // Set DeletedBy if we have access to current user service
        if (_context is IApplicationDbContext appContext)
        {
            // This would require injecting ICurrentUserService into the repository
            // For now, we'll leave it as null or implement it at the service layer
        }
        
        Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // This requires querying including soft deleted entities
        var entity = await _dbSet.IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        
        if (entity != null && entity.IsDeleted)
        {
            entity.IsDeleted = false;
            entity.DeletedAt = null;
            entity.DeletedBy = null;
            Update(entity);
        }
    }

    // Include operations for navigation properties
    public virtual IQueryable<T> Include(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        
        return query;
    }

    public virtual async Task<T?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<T, object>>[] includes)
    {
        var query = Include(includes);
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes)
    {
        var query = Include(includes);
        return await query.ToListAsync();
    }
}