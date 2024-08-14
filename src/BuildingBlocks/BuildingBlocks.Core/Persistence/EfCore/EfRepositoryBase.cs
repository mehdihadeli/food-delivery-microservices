using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace BuildingBlocks.Core.Persistence.EfCore;

public abstract class EfRepositoryBase<TDbContext, TEntity, TKey>(TDbContext dbContext, ISieveProcessor sieveProcessor)
    : IRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentity<TKey>
    where TDbContext : DbContext
{
    protected DbSet<TEntity> DbSet { get; } = dbContext.Set<TEntity>();
    protected TDbContext DbContext { get; } = dbContext;

    public Task<TEntity?> FindByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return DbSet.SingleOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }

    public Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        predicate.NotBeNull();

        return DbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet.AnyAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public IAsyncEnumerable<TResult> ProjectBy<TResult, TSortKey>(
        IConfigurationProvider configuration,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = DbSet.AsQueryable();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (sortExpression is not null)
        {
            query = query.OrderByDescending(sortExpression);
        }

        return query.ProjectTo<TResult>(configuration).ToAsyncEnumerable();
    }

    public async Task<IPageList<TEntity>> GetByPageFilter<TSortKey>(
        IPageRequest pageRequest,
        Expression<Func<TEntity, TSortKey>> sortExpression,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet.ApplyPagingAsync(pageRequest, sieveProcessor, predicate, sortExpression, cancellationToken);
    }

    public async Task<IPageList<TResult>> GetByPageFilter<TResult, TSortKey>(
        IPageRequest pageRequest,
        IConfigurationProvider configuration,
        Expression<Func<TEntity, TSortKey>> sortExpression,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        return await DbSet.ApplyPagingAsync<TEntity, TResult, TSortKey>(
            pageRequest,
            sieveProcessor,
            configuration,
            predicate,
            sortExpression,
            cancellationToken
        );
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.NotBeNull();

        await DbSet.AddAsync(entity, cancellationToken);

        return entity;
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.NotBeNull();

        var entry = DbContext.Entry(entity);
        entry.State = EntityState.Modified;

        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        entities.NotBeNull();

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
        entity.NotBeNull();

        DbSet.Remove(entity);

        return Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var item = await DbSet.SingleOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        if (item is null)
            throw new NotFoundException($"Item with ID '{id}' not found");

        DbSet.Remove(item);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
