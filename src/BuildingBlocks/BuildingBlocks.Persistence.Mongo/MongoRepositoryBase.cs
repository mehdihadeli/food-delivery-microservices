using System.Linq.Expressions;
using Ardalis.Specification;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Sieve.Services;

namespace BuildingBlocks.Persistence.Mongo;

public class MongoRepositoryBase<TDbContext, TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class, Abstractions.Domain.IEntity<TId>
    where TDbContext : MongoDbContext
{
    private readonly ISieveProcessor _sieveProcessor;

    protected TDbContext Context { get; }
    protected IMongoCollection<TEntity> DbSet { get; }

    public MongoRepositoryBase(TDbContext context, ISieveProcessor sieveProcessor)
    {
        Context = context;
        _sieveProcessor = sieveProcessor;
        DbSet = Context.GetCollection<TEntity>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<TEntity?> FindByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
        return await DbSet.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return DbSet.Find(predicate).SingleOrDefaultAsync(cancellationToken: cancellationToken)!;
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet.Find(predicate).ToListAsync(cancellationToken: cancellationToken)!;
    }

    public Task<bool> AnyAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        var filter = Builders<TEntity>.Filter.Where(predicate);
        var count = await DbSet.CountDocumentsAsync(filter, null, cancellationToken);
        return count > 0;
    }

    public IQueryable<TResult> ProjectBy<TResult, TSortKey>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> projectionFunc,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null
    )
        where TResult : class
    {
        var query = DbSet.AsQueryable().Project(projectionFunc, sortExpression, predicate);

        return query;
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsQueryable().ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<TEntity>> GetAllAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TResult>> GetAllAsync<TResult>(
        ISpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<TResult?> FirstOrDefaultAsync<TResult>(
        ISpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<TEntity?> SingleOrDefaultAsync(
        ISingleResultSpecification<TEntity> specification,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<TResult?> SingleOrDefaultAsync<TResult>(
        ISingleResultSpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return DbSet.AsQueryable().CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<IPageList<TEntity>> GetByPageFilter<TSortKey>(
        IPageRequest pageRequest,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .AsQueryable()
            .ApplyPagingAsync(pageRequest, _sieveProcessor, sortExpression, predicate, cancellationToken);
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
        return await DbSet
            .AsQueryable()
            .ApplyPagingAsync(
                pageRequest,
                _sieveProcessor,
                projectionFunc,
                sortExpression,
                predicate,
                cancellationToken
            );
    }

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.AddCommand(async () =>
        {
            await DbSet.InsertOneAsync(entity, null, cancellationToken);
        });

        return Task.FromResult(entity);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.AddCommand(async () =>
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id);
            await DbSet.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        });

        return Task.FromResult(entity);
    }

    public Task DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Context.AddCommand(async () =>
        {
            var idsToDelete = entities.Select(x => x.Id).ToList();
            var filter = Builders<TEntity>.Filter.In(x => x.Id, idsToDelete);
            await DbSet.DeleteManyAsync(filter, cancellationToken);
        });

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        Context.AddCommand(async () =>
        {
            await DbSet.DeleteOneAsync(predicate, cancellationToken);
        });

        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.AddCommand(async () =>
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id);
            await DbSet.DeleteOneAsync(filter, cancellationToken);
        });

        return Task.CompletedTask;
    }

    public Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        Context.AddCommand(async () =>
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
            await DbSet.DeleteOneAsync(filter, cancellationToken);
        });

        return Task.CompletedTask;
    }
}
