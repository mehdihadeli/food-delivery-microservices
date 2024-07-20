using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Sieve.Services;

namespace BuildingBlocks.Persistence.Mongo;

public class MongoRepositoryBase<TDbContext, TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class, IHaveIdentity<TId>
    where TDbContext : MongoDbContext
{
    private readonly TDbContext _context;
    private readonly ISieveProcessor _sieveProcessor;
    protected readonly IMongoCollection<TEntity> DbSet;

    public MongoRepositoryBase(TDbContext context, ISieveProcessor sieveProcessor)
    {
        _context = context;
        _sieveProcessor = sieveProcessor;
        DbSet = _context.GetCollection<TEntity>();
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

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        var filter = Builders<TEntity>.Filter.Where(predicate);
        var count = await DbSet.CountDocumentsAsync(filter, null, cancellationToken);
        return count > 0;
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsQueryable().ToListAsync(cancellationToken);
    }

    public IAsyncEnumerable<TResult> ProjectBy<TResult, TSortKey>(
        IConfigurationProvider configuration,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        CancellationToken cancellationToken = default
    )
    {
        IMongoQueryable<TEntity> query = DbSet.AsQueryable();
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
        return await DbSet
            .AsQueryable()
            .ApplyPagingAsync(pageRequest, _sieveProcessor, predicate, sortExpression, cancellationToken);
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
        return await DbSet
            .AsQueryable()
            .ApplyPagingAsync<TEntity, TResult, TSortKey>(
                pageRequest,
                _sieveProcessor,
                configuration,
                predicate,
                sortExpression,
                cancellationToken
            );
    }

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.AddCommand(async () =>
        {
            await DbSet.InsertOneAsync(entity, null, cancellationToken);
        });

        return Task.FromResult(entity);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.AddCommand(async () =>
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id);
            await DbSet.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        });

        return Task.FromResult(entity);
    }

    public Task DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _context.AddCommand(async () =>
        {
            var idsToDelete = entities.Select(x => x.Id).ToList();
            var filter = Builders<TEntity>.Filter.In(x => x.Id, idsToDelete);
            await DbSet.DeleteManyAsync(filter, cancellationToken);
        });

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        _context.AddCommand(async () =>
        {
            await DbSet.DeleteOneAsync(predicate, cancellationToken);
        });

        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.AddCommand(async () =>
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id);
            await DbSet.DeleteOneAsync(filter, cancellationToken);
        });

        return Task.CompletedTask;
    }

    public Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        _context.AddCommand(async () =>
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
            await DbSet.DeleteOneAsync(filter, cancellationToken);
        });

        return Task.CompletedTask;
    }
}
