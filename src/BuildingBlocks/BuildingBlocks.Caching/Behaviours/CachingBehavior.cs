using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Caching;
using EasyCaching.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Caching.Behaviours;

// Ref: https://anderly.com/2019/12/12/cross-cutting-concerns-with-mediatr-pipeline-behaviors/
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly IEasyCachingProvider _cacheProvider;
    private readonly IEnumerable<ICacheRequest<TRequest, TResponse>> _cachePolicies;

    public CachingBehavior(
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<CacheOptions> cacheOptions,
        IEnumerable<ICacheRequest<TRequest, TResponse>> cachePolicies)
    {
        _logger = Guard.Against.Null(logger);
        Guard.Against.Null(cacheOptions.Value);
        _cacheProvider = Guard.Against.Null(cachingProviderFactory).GetCachingProvider(cacheOptions.Value.DefaultCacheType);

        // cachePolicies inject like `FluentValidation` approach as a nested or seperated cache class for commands ,queries
        _cachePolicies = cachePolicies;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheRequest = _cachePolicies.FirstOrDefault();
        if (cacheRequest == null)
        {
            // No cache policy found, so just continue through the pipeline
            return await next();
        }

        var cacheKey = cacheRequest.CacheKey(request);
        var cachedResponse = await _cacheProvider.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResponse.Value != null)
        {
            _logger.LogDebug(
                "Response retrieved {TRequest} from cache. CacheKey: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey);
            return cachedResponse.Value;
        }

        var response = await next();

        await _cacheProvider.SetAsync(
            cacheKey,
            response,
            cacheRequest.AbsoluteExpirationRelativeToNow,
            cancellationToken);

        _logger.LogDebug(
            "Caching response for {TRequest} with cache key: {CacheKey}",
            typeof(TRequest).FullName,
            cacheKey);

        return response;
    }
}

public class StreamCachingBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<StreamCachingBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<ICacheRequest<TRequest, TResponse>> _cachePolicies;
    private readonly IEasyCachingProvider _cacheProvider;

    public StreamCachingBehavior(
        ILogger<StreamCachingBehavior<TRequest, TResponse>> logger,
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<CacheOptions> cacheOptions,
        IEnumerable<ICacheRequest<TRequest, TResponse>> cachePolicies)
    {
        _logger = Guard.Against.Null(logger);
        Guard.Against.Null(cacheOptions.Value);
        _cacheProvider = Guard.Against.Null(cachingProviderFactory).GetCachingProvider(cacheOptions.Value.DefaultCacheType);

        // cachePolicies inject like `FluentValidation` approach as a nested or seperated cache class for commands ,queries
        _cachePolicies = cachePolicies;
    }

    public IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheRequest = _cachePolicies.FirstOrDefault();
        if (cacheRequest == null)
        {
            return next();
        }

        var cacheKey = cacheRequest.CacheKey(request);
        var cachedResponse = _cacheProvider.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            _logger.LogDebug(
                "Response retrieved {TRequest} from cache. CacheKey: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey);
            return next();
        }

        var response = next();

        _cacheProvider.SetAsync(
            cacheKey,
            response,
            cacheRequest.AbsoluteExpirationRelativeToNow,
            cancellationToken).GetAwaiter().GetResult();

        _logger.LogDebug(
            "Caching response for {TRequest} with cache key: {CacheKey}",
            typeof(TRequest).FullName,
            cacheKey);

        return response;
    }
}
