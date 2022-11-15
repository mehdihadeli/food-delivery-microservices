using BuildingBlocks.Abstractions.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Caching;

// Ref: https://anderly.com/2019/12/12/cross-cutting-concerns-with-mediatr-pipeline-behaviors/
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly int _defaultCacheExpirationInHours = 1;

    public CachingBehavior(
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        ICacheManager cacheManager,
        IEnumerable<ICacheRequest<TRequest, TResponse>> cachePolicies)
    {
        _logger = logger;
        _cacheProvider = cacheManager.DefaultCacheProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken
        cancellationToken)
    {
        if (request is not ICacheRequest<TRequest, TResponse> cachePolicy)
            return await next();

        var cacheKey = cachePolicy.GetCacheKey(request);
        var cachedResponse = await _cacheProvider.GetAsync<TResponse?>(cacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            _logger.LogDebug(
                "Response retrieved {TRequest} from cache. CacheKey: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey);
            return cachedResponse;
        }

        var response = await next();

        await _cacheProvider.SetAsync(
            cacheKey,
            response,
            cachePolicy.SlidingExpiration,
            cachePolicy.AbsoluteExpiration,
            cachePolicy.AbsoluteExpirationRelativeToNow,
            cancellationToken);

        _logger.LogDebug(
            "Caching response for {TRequest} with cache key: {CacheKey}",
            typeof(TRequest).FullName,
            cacheKey);

        return response;
    }
}

public class StreamCachingBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IStreamRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<StreamCachingBehavior<TRequest, TResponse>> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly int defaultCacheExpirationInHours = 1;

    public StreamCachingBehavior(ILogger<StreamCachingBehavior<TRequest, TResponse>> logger, ICacheManager cacheManager)
    {
        _logger = logger;
        _cacheProvider = cacheManager.DefaultCacheProvider;
    }

    public IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IStreamCacheRequest<TRequest, TResponse> cachePolicy)
            return next();

        var cacheKey = cachePolicy.GetCacheKey(request);
        var cachedResponse = _cacheProvider.GetAsync<TResponse?>(cacheKey, cancellationToken);
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
            cachePolicy.SlidingExpiration,
            cachePolicy.AbsoluteExpiration,
            cachePolicy.AbsoluteExpirationRelativeToNow,
            cancellationToken).GetAwaiter().GetResult();

        _logger.LogDebug(
            "Caching response for {TRequest} with cache key: {CacheKey}",
            typeof(TRequest).FullName,
            cacheKey);

        return response;
    }
}
