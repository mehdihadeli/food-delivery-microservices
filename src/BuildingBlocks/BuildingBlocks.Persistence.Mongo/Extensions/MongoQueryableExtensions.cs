using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Core.Paging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Sieve.Models;
using Sieve.Services;

namespace BuildingBlocks.Persistence.Mongo.Extensions;

public static class MongoQueryableExtensions
{
    public static async Task<IPageList<TEntity>> ApplyPagingAsync<TEntity, TSortKey>(
        this IMongoCollection<TEntity> collection,
        IPageRequest pageRequest,
        ISieveProcessor sieveProcessor,
        Expression<Func<TEntity, TSortKey>> sortExpression,
        CancellationToken cancellationToken
    )
        where TEntity : class
    {
        IQueryable<TEntity> queryable = collection.AsQueryable();
        queryable = queryable.OrderByDescending(sortExpression);

        var sieveModel = new SieveModel
        {
            PageSize = pageRequest.PageSize,
            Page = pageRequest.PageNumber,
            Sorts = pageRequest.SortOrder,
            Filters = pageRequest.Filters,
        };

        // https://github.com/Biarity/Sieve/issues/34#issuecomment-403817573
        var result = sieveProcessor.Apply(sieveModel, queryable, applyPagination: false);

        // Ensure ordering is applied before applying pagination
        if (!result.Expression.ToString().Contains("OrderBy", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The query must include an 'OrderBy' clause before pagination.");
        }

        // The provider for the source 'IQueryable' doesn't implement 'IAsyncQueryProvider'. Only providers that implement 'IAsyncQueryProvider' can be used for Entity Framework asynchronous operations.
        var total = result.Count();
        result = sieveProcessor.Apply(sieveModel, queryable, applyFiltering: false, applySorting: false);
        var items = await result.ToAsyncEnumerable().ToListAsync(cancellationToken: cancellationToken);

        return PageList<TEntity>.Create(items.AsReadOnly(), pageRequest.PageNumber, pageRequest.PageSize, total);
    }

    public static async Task<IPageList<TResult>> ApplyPagingAsync<TEntity, TResult, TSortKey>(
        this IMongoCollection<TEntity> collection,
        IPageRequest pageRequest,
        ISieveProcessor sieveProcessor,
        Func<IQueryable<TEntity>, IQueryable<TResult>> projectionFunc,
        Expression<Func<TEntity, TSortKey>> sortExpression,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
        where TResult : class
    {
        IQueryable<TEntity> queryable = collection.AsQueryable();
        if (predicate is not null)
        {
            queryable = queryable.Where(predicate);
        }

        queryable = queryable.OrderByDescending(sortExpression);

        var sieveModel = new SieveModel
        {
            PageSize = pageRequest.PageSize,
            Page = pageRequest.PageNumber,
            Sorts = pageRequest.SortOrder,
            Filters = pageRequest.Filters,
        };

        // https://github.com/Biarity/Sieve/issues/34#issuecomment-403817573
        IQueryable<TEntity> result = sieveProcessor.Apply(sieveModel, queryable, applyPagination: false);

        // The provider for the source 'IQueryable' doesn't implement 'IAsyncQueryProvider'. Only providers that implement 'IAsyncQueryProvider' can be used for Entity Framework asynchronous operations.
        var total = result.Count();
        result = sieveProcessor.Apply(sieveModel, queryable, applyFiltering: false, applySorting: true); // Only applies pagination
        var projectedQuery = projectionFunc(result);

        var items = await projectedQuery.ToAsyncEnumerable().ToListAsync(cancellationToken: cancellationToken);

        return PageList<TResult>.Create(items.AsReadOnly(), pageRequest.PageNumber, pageRequest.PageSize, total);
    }

    public static async Task<IPageList<TResult>> ApplyPagingAsync<TEntity, TResult, TSortKey>(
        this IMongoCollection<TEntity> collection,
        IPageRequest pageRequest,
        ISieveProcessor sieveProcessor,
        Func<TEntity, TResult> map,
        Expression<Func<TEntity, TSortKey>> sortExpression,
        CancellationToken cancellationToken
    )
        where TEntity : class
        where TResult : class
    {
        IQueryable<TEntity> queryable = collection.AsQueryable();
        queryable = queryable.OrderByDescending(sortExpression);

        var sieveModel = new SieveModel
        {
            PageSize = pageRequest.PageSize,
            Page = pageRequest.PageNumber,
            Sorts = pageRequest.SortOrder,
            Filters = pageRequest.Filters,
        };

        // https://github.com/Biarity/Sieve/issues/34#issuecomment-403817573
        var result = sieveProcessor.Apply(sieveModel, queryable, applyPagination: false);
        // The provider for the source 'IQueryable' doesn't implement 'IAsyncQueryProvider'. Only providers that implement 'IAsyncQueryProvider' can be used for Entity Framework asynchronous operations.
        var total = result.Count();
        result = sieveProcessor.Apply(sieveModel, queryable, applyFiltering: false, applySorting: false); // Only applies pagination

        var items = await result
            .Select(x => map(x))
            .ToAsyncEnumerable()
            .ToListAsync(cancellationToken: cancellationToken);

        return PageList<TResult>.Create(items.AsReadOnly(), pageRequest.PageNumber, pageRequest.PageSize, total);
    }

    public static async Task<IPageList<TEntity>> ApplyPagingAsync<TEntity, TSortKey>(
        this IMongoCollection<TEntity> collection,
        IPageRequest pageRequest,
        ISieveProcessor sieveProcessor,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        IQueryable<TEntity> queryable =
            predicate != null ? collection.AsQueryable().Where(predicate) : collection.AsQueryable();

        if (sortExpression != null)
        {
            queryable = queryable.OrderByDescending(sortExpression);
        }

        var sieveModel = new SieveModel
        {
            PageSize = pageRequest.PageSize,
            Page = pageRequest.PageNumber,
            Sorts = pageRequest.SortOrder,
            Filters = pageRequest.Filters,
        };

        var result = sieveProcessor.Apply(sieveModel, queryable, applyPagination: false);
        var total = result.Count();
        result = sieveProcessor.Apply(sieveModel, queryable, applyFiltering: false, applySorting: false);

        var items = await result.ToListAsync(cancellationToken: cancellationToken);

        return PageList<TEntity>.Create(items.AsReadOnly(), pageRequest.PageNumber, pageRequest.PageSize, total);
    }
}
