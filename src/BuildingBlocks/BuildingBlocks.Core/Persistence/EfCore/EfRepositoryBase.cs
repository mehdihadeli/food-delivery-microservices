using System.Linq.Expressions;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BuildingBlocks.Core.Persistence.EfCore;

public abstract class EfRepositoryBase<TDbContext, TEntity, TKey> :
    IEfRepository<TEntity, TKey>,
    IPageRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentity<TKey>
    where TDbContext : DbContext
{
    protected readonly TDbContext DbContext;
    private readonly IAggregatesDomainEventsRequestStore _aggregatesDomainEventsStore;
    protected readonly DbSet<TEntity> DbSet;

    protected EfRepositoryBase(TDbContext dbContext, IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
    {
        DbContext = dbContext;
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
        DbSet = dbContext.Set<TEntity>();
    }

    public Task<TEntity?> FindByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return DbSet.SingleOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }

    public Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(predicate, nameof(predicate));

        return DbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual IEnumerable<TEntity> GetInclude(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null, bool withTracking = true)
    {
        IQueryable<TEntity> query = DbSet;

        if (includes != null)
        {
            query = includes(query);
        }

        query = query.Where(predicate);

        if (withTracking == false)
        {
            query = query.Where(predicate).AsNoTracking();
        }

        return query.AsEnumerable();
    }

    public virtual IEnumerable<TEntity> GetInclude(
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null)
    {
        IQueryable<TEntity> query = DbSet;

        if (includes != null)
        {
            query = includes(query);
        }

        return query.AsEnumerable();
    }

    public virtual async Task<IEnumerable<TEntity>> GetIncludeAsync(
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null)
    {
        IQueryable<TEntity> query = DbSet;

        if (includes != null)
        {
            query = includes(query);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetIncludeAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null,
        bool withTracking = true)
    {
        IQueryable<TEntity> query = DbSet;

        if (includes != null)
        {
            query = includes(query);
        }

        query = query.Where(predicate);

        if (withTracking == false)
        {
            query = query.Where(predicate).AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        await DbSet.AddAsync(entity, cancellationToken);

        return entity;
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        var entry = DbContext.Entry(entity);
        entry.State = EntityState.Modified;

        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(entities, nameof(entities));

        foreach (var entity in entities)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var items = DbSet.Where(predicate).ToList();

        return DeleteRangeAsync(items, cancellationToken);
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        DbSet.Remove(entity);

        return Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var item = await DbSet.SingleOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        Guard.Against.NotFound(id.ToString(), id.ToString(), nameof(id));

        DbSet.Remove(item);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
