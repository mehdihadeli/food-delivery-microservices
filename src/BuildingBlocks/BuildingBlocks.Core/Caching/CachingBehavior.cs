using BuildingBlocks.Abstractions.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Caching;

// Ref: https://anderly.com/2019/12/12/cross-cutting-concerns-with-mediatr-pipeline-behaviors/
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly IEnumerable<ICachePolicy<TRequest, TResponse>> _cachePolicies;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly ICacheManager _cacheManager;
    private readonly int defaultCacheExpirationInHours = 1;

    public CachingBehavior(
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        ICacheManager cacheManager,
        IEnumerable<ICachePolicy<TRequest, TResponse>> cachePolicies)
    {
        _logger = logger;
        _cacheManager = cacheManager;
        _cachePolicies = cachePolicies;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var cachePolicy = _cachePolicies.FirstOrDefault();
        if (cachePolicy == null)
        {
            // No cache policy found, so just continue through the pipeline
            return await next();
        }

        var cacheKey = cachePolicy.GetCacheKey(request);
        var time = cachePolicy.AbsoluteExpirationRelativeToNow ??
                   DateTime.Now.AddHours(defaultCacheExpirationInHours);
        var cachedResponse = await _cacheManager.GetAsync<TResponse>(cacheKey);

        if (cachedResponse is { })
        {
            _logger.LogDebug(
                "Response retrieved {TRequest} from cache. CacheKey: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey);
            return cachedResponse;
        }

        var response = await next();


        await _cacheManager.SetAsync(cacheKey, response, time.Second);

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
    private readonly IEnumerable<IStreamCachePolicy<TRequest, TResponse>> _cachePolicies;
    private readonly ILogger<StreamCachingBehavior<TRequest, TResponse>> _logger;
    private readonly ICacheManager _cacheManager;
    private readonly int defaultCacheExpirationInHours = 1;

    public StreamCachingBehavior(
        ILogger<StreamCachingBehavior<TRequest, TResponse>> logger,
        ICacheManager cacheManager,
        IEnumerable<IStreamCachePolicy<TRequest, TResponse>> cachePolicies)
    {
        _logger = logger;
        _cacheManager = cacheManager;
        _cachePolicies = cachePolicies;
    }

    public IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        StreamHandlerDelegate<TResponse> next)
    {
        var cachePolicy = _cachePolicies.FirstOrDefault();
        if (cachePolicy == null)
        {
            // No cache policy found, so just continue through the pipeline
            return next();
        }

        var cacheKey = cachePolicy.GetCacheKey(request);
        var cachedResponse = _cacheManager.Get<IAsyncEnumerable<TResponse>>(cacheKey);
        if (cachedResponse is { })
        {
            _logger.LogDebug(
                "Response retrieved {TRequest} from cache. CacheKey: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey);
            return cachedResponse;
        }

        var response = next();

        var time = cachePolicy.AbsoluteExpirationRelativeToNow ??
                   DateTime.Now.AddHours(defaultCacheExpirationInHours);

        _cacheManager.Set(cacheKey, response, time.TimeOfDay.Seconds);

        _logger.LogDebug(
            "Caching response for {TRequest} with cache key: {CacheKey}",
            typeof(TRequest).FullName,
            cacheKey);

        return response;
    }
}
