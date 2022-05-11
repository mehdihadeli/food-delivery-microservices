using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Core.Linq;
using BuildingBlocks.Core.Types;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Core.Persistence.EfCore;

// https://github.com/nreco/lambdaparser
// https://github.com/dynamicexpresso/DynamicExpresso
public static class EfCoreQueryableExtensions
{
    public static async Task<ListResultModel<T>> ApplyPagingAsync<T>(
        this IQueryable<T> collection,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
        if (page <= 0) page = 1;

        if (pageSize <= 0) pageSize = 10;

        var isEmpty = await collection.AnyAsync(cancellationToken: cancellationToken) == false;
        if (isEmpty) return ListResultModel<T>.Empty;

        var totalItems = await collection.CountAsync(cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
        var data = await collection.Limit(page, pageSize).ToListAsync(cancellationToken: cancellationToken);

        return ListResultModel<T>.Create(data, totalItems, page, pageSize);
    }

    public static async Task<ListResultModel<TR>> ApplyPagingAsync<T, TR>(
        this IQueryable<T> collection,
        IConfigurationProvider configuration,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
        where TR : notnull
    {
        if (page <= 0) page = 1;

        if (pageSize <= 0) pageSize = 10;

        var isEmpty = await collection.AnyAsync(cancellationToken: cancellationToken) == false;
        if (isEmpty) return ListResultModel<TR>.Empty;

        var totalItems = await collection.CountAsync(cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
        var data = await collection.Limit(page, pageSize).ProjectTo<TR>(configuration)
            .ToListAsync(cancellationToken: cancellationToken);

        return ListResultModel<TR>.Create(data, totalItems, page, pageSize);
    }

    public static IQueryable<TEntity> ApplyPaging<TEntity>(
        this IQueryable<TEntity> source,
        int page,
        int size)
        where TEntity : class
    {
        return source.Skip(page * size).Take(size);
    }

    public static IQueryable<T> Limit<T>(
        this IQueryable<T> collection,
        int page = 1,
        int resultsPerPage = 10)
    {
        if (page <= 0) page = 1;

        if (resultsPerPage <= 0) resultsPerPage = 10;

        var skip = (page - 1) * resultsPerPage;
        var data = collection.Skip(skip)
            .Take(resultsPerPage);

        return data;
    }

    public static IQueryable<TEntity> ApplyFilter<TEntity>(
        this IQueryable<TEntity> source,
        IEnumerable<FilterModel>? filters)
        where TEntity : class
    {
        if (filters is null)
            return source;

        List<Expression<Func<TEntity, bool>>> filterExpressions = new List<Expression<Func<TEntity, bool>>>();

        foreach (var (fieldName, comparision, fieldValue) in filters)
        {
            Expression<Func<TEntity, bool>> expr = PredicateBuilder.Build<TEntity>(fieldName, comparision, fieldValue);
            filterExpressions.Add(expr);
        }

        return source.Where(filterExpressions.Aggregate((expr1, expr2) => expr1.And(expr2)));
    }

    public static IQueryable<TEntity> ApplyIncludeList<TEntity>(
        this IQueryable<TEntity> source,
        IEnumerable<string>? navigationPropertiesPath)
        where TEntity : class
    {
        if (navigationPropertiesPath is null)
            return source;

        foreach (var navigationPropertyPath in navigationPropertiesPath)
        {
            source = source.Include(navigationPropertyPath);
        }

        return source;
    }
}
