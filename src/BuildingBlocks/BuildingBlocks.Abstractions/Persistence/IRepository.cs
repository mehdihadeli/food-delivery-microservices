using System.Linq.Expressions;
using Ardalis.Specification;
using BuildingBlocks.Abstractions.Core.Paging;

namespace BuildingBlocks.Abstractions.Persistence;

public interface IReadRepository<TEntity, in TId>
    where TEntity : class, Domain.IEntity<TId>
{
    Task<TEntity?> FindByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<TResult>> GetAllAsync<TResult>(
        ISpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default
    );

    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    );

    Task<TResult?> FirstOrDefaultAsync<TResult>(
        ISpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default
    );

    Task<TEntity?> SingleOrDefaultAsync(
        ISingleResultSpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    );

    Task<TResult?> SingleOrDefaultAsync<TResult>(
        ISingleResultSpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    IQueryable<TResult> ProjectBy<TResult, TSortKey>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> projectionFunc,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null
    )
        where TResult : class;

    Task<IPageList<TEntity>> GetByPageFilter<TSortKey>(
        IPageRequest pageRequest,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );
    Task<IPageList<TResult>> GetByPageFilter<TSortKey, TResult>(
        IPageRequest pageRequest,
        Func<IQueryable<TEntity>, IQueryable<TResult>> projectionFunc,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
        where TResult : class;
}

public interface IWriteRepository<TEntity, in TId>
    where TEntity : class, Domain.IEntity<TId>
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default);
}

public interface IRepository<TEntity, in TId>
    : IReadRepository<TEntity, TId>,
        IWriteRepository<TEntity, TId>,
        IDisposable
    where TEntity : class, Domain.IEntity<TId>;

public interface IRepository<TEntity> : IRepository<TEntity, long>
    where TEntity : class, Domain.IEntity<long>;
