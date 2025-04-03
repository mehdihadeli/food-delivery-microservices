using System.Linq.Expressions;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Core.Paging;
using Sieve.Models;
using Sieve.Services;

namespace BuildingBlocks.Core.Extensions;

// we should not relate to Ef or Mongo here, and we should design as general with IQueryable to work with any providers
public static class QueryableExtensions
{
    public static async Task<IPageList<TResult>> ApplyPagingAsync<TEntity, TResult, TSortKey>(
        this IQueryable<TEntity> collection,
        IPageRequest pageRequest,
        ISieveProcessor sieveProcessor,
        Func<IQueryable<TEntity>, IQueryable<TResult>> projectionFunc,
        Expression<Func<TEntity, TSortKey>>? sortExpression,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
        where TResult : class
    {
        IQueryable<TEntity> queryable = collection;
        if (predicate is not null)
        {
            queryable = queryable.Where(predicate);
        }

        if (sortExpression is not null)
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

        // https://github.com/Biarity/Sieve/issues/34#issuecomment-403817573
        var result = sieveProcessor.Apply(sieveModel, queryable, applyPagination: false);
        // The provider for the source 'IQueryable' doesn't implement 'IAsyncQueryProvider'. Only providers that implement 'IAsyncQueryProvider' can be used for Entity Framework asynchronous operations.
        var total = result.Count();
        result = sieveProcessor.Apply(sieveModel, queryable, applyFiltering: false, applySorting: false); // Only applies pagination

        var projectedQuery = projectionFunc(result);

        var items = await projectedQuery.ToAsyncEnumerable().ToListAsync(cancellationToken: cancellationToken);

        return PageList<TResult>.Create(items.AsReadOnly(), pageRequest.PageNumber, pageRequest.PageSize, total);
    }

    public static async Task<IPageList<TEntity>> ApplyPagingAsync<TEntity, TSortKey>(
        this IQueryable<TEntity> collection,
        IPageRequest pageRequest,
        ISieveProcessor sieveProcessor,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
    {
        IQueryable<TEntity> queryable = predicate != null ? collection.Where(predicate) : collection;

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

        var items = await result.ToAsyncEnumerable().ToListAsync(cancellationToken: cancellationToken);

        return PageList<TEntity>.Create(items.AsReadOnly(), pageRequest.PageNumber, pageRequest.PageSize, total);
    }

    public static IQueryable<TResult> Project<TEntity, TResult, TSortKey>(
        this IQueryable<TEntity> collection,
        Func<IQueryable<TEntity>, IQueryable<TResult>> projectionFunc,
        Expression<Func<TEntity, TSortKey>>? sortExpression = null,
        Expression<Func<TEntity, bool>>? predicate = null
    )
        where TEntity : class
        where TResult : class
    {
        IQueryable<TEntity> queryable = collection;
        if (predicate is not null)
        {
            queryable = queryable.Where(predicate);
        }

        if (sortExpression is not null)
        {
            queryable = queryable.OrderByDescending(sortExpression);
        }

        var items = projectionFunc(queryable);

        return items;
    }
}
