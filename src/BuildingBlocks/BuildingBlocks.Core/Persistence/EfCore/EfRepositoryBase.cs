using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace BuildingBlocks.Core.Persistence.EfCore;

public abstract class EfRepositoryBase<TDbContext, TEntity, TKey>(TDbContext dbContext, ISieveProcessor sieveProcessor)
    : IRepository<TEntity, TKey>
    where TEntity : class, Abstractions.Domain.IEntity<TKey>
    where TDbContext : DbContext
{
    private readonly SpecificationEvaluator _specificationEvaluator = SpecificationEvaluator.Default;

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
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await ApplySpecification(specification, true).AnyAsync(cancellationToken);
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

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        var queryResult = await ApplySpecification(specification).ToListAsync(cancellationToken);

        return specification.PostProcessingAction is null
            ? queryResult
            : specification.PostProcessingAction(queryResult).ToList();
    }

    public async Task<IReadOnlyList<TResult>> GetAllAsync<TResult>(
        ISpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default
    )
    {
        var queryResult = await ApplySpecification(specification).ToListAsync(cancellationToken);

        return specification.PostProcessingAction is null
            ? queryResult
            : specification.PostProcessingAction(queryResult).ToList();
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(
        ISpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        ISingleResultSpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TResult?> SingleOrDefaultAsync<TResult>(
        ISingleResultSpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    public IQueryable<TResult> ProjectBy<TResult, TSortKey>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> projectionFunc,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null
    )
        where TResult : class
    {
        var query = DbSet.Project(projectionFunc, sortExpression, predicate);

        return query;
    }

    public async Task<IPageList<TEntity>> GetByPageFilter<TSortKey>(
        IPageRequest pageRequest,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet.ApplyPagingAsync(pageRequest, sieveProcessor, sortExpression, predicate, cancellationToken);
    }

    public async Task<IPageList<TResult>> GetByPageFilter<TSortKey, TResult>(
        IPageRequest pageRequest,
        Func<IQueryable<TEntity>, IQueryable<TResult>> projectionFunc,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        return await DbSet.ApplyPagingAsync(
            pageRequest,
            sieveProcessor,
            projectionFunc,
            sortExpression,
            predicate,
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

    private IQueryable<TEntity> ApplySpecification(
        ISpecification<TEntity> specification,
        bool evaluateCriteriaOnly = false
    ) => _specificationEvaluator.GetQuery(dbContext.Set<TEntity>().AsQueryable(), specification, evaluateCriteriaOnly);

    private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> specification) =>
        _specificationEvaluator.GetQuery(dbContext.Set<TEntity>().AsQueryable(), specification);
}
