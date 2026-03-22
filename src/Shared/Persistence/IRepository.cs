using Shared.Domain;
using System.Linq.Expressions;

namespace Shared.Persistence;

public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    Task<TEntity?> GetByIdAsync(
    TId id,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
    bool asNoTracking = false,
    CancellationToken ct = default);

    Task<IEnumerable<TEntity>> GetAllAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = false,
        CancellationToken ct = default);
    
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = false,
        CancellationToken ct = default);
    
    Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool asNoTracking = false,
        CancellationToken ct = default);
    
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = false,
        CancellationToken ct = default);
    
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool asNoTracking = false,
        CancellationToken ct = default);
    
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    
    Task SaveChangesAsync(CancellationToken ct = default);

    void Update(TEntity entity);
    
    void Remove(TEntity entity);
    
    void RemoveRange(IEnumerable<TEntity> entities);
}
