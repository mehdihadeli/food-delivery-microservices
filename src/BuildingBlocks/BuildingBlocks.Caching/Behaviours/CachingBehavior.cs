using BuildingBlocks.Abstractions.Caching;
using EasyCaching.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Caching.Behaviours;

// Ref: https://anderly.com/2019/12/12/cross-cutting-concerns-with-mediatr-pipeline-behaviors/
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly IEasyCachingProvider _cacheProvider;
    private readonly CacheOptions _cacheOptions;

    public CachingBehavior(
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<CacheOptions> cacheOptions
    )
    {
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
        _cacheProvider = cachingProviderFactory.GetCachingProvider(cacheOptions.Value.DefaultCacheType);
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not ICacheRequest<TRequest, TResponse> cacheRequest)
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
                cacheKey
            );
            return cachedResponse.Value;
        }

        var response = await next();

        var expiredTimeSpan =
            cacheRequest.AbsoluteExpirationRelativeToNow != TimeSpan.FromMinutes(5)
                ? cacheRequest.AbsoluteExpirationRelativeToNow
                : TimeSpan.FromMinutes(_cacheOptions.ExpirationTimeInMinute) != TimeSpan.FromMinutes(5)
                    ? TimeSpan.FromMinutes(_cacheOptions.ExpirationTimeInMinute)
                    : cacheRequest.AbsoluteExpirationRelativeToNow;

        await _cacheProvider.SetAsync(cacheKey, response, expiredTimeSpan, cancellationToken);

        _logger.LogDebug(
            "Caching response for {TRequest} with cache key: {CacheKey}",
            typeof(TRequest).FullName,
            cacheKey
        );

        return response;
    }
}

public class StreamCachingBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TResponse : class
{
    private readonly ILogger<StreamCachingBehavior<TRequest, TResponse>> _logger;
    private readonly IEasyCachingProvider _cacheProvider;
    private readonly CacheOptions _cacheOptions;

    public StreamCachingBehavior(
        ILogger<StreamCachingBehavior<TRequest, TResponse>> logger,
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<CacheOptions> cacheOptions
    )
    {
        _cacheOptions = cacheOptions.Value;
        _logger = logger;
        _cacheProvider = cachingProviderFactory.GetCachingProvider(cacheOptions.Value.DefaultCacheType);
    }

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not IStreamCacheRequest<TRequest, TResponse> cacheRequest)
        {
            // If the request does not implement IStreamCacheRequest, go to the next pipeline
            await foreach (var response in next().WithCancellation(cancellationToken))
            {
                yield return response;
            }

            yield break;
        }

        var cacheKey = cacheRequest.CacheKey(request);
        var cachedResponse = _cacheProvider.Get<TResponse>(cacheKey);

        if (cachedResponse != null)
        {
            _logger.LogDebug(
                "Response retrieved {TRequest} from cache. CacheKey: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey
            );

            yield return cachedResponse.Value;
            yield break;
        }

        var expiredTimeSpan =
            cacheRequest.AbsoluteExpirationRelativeToNow != TimeSpan.FromMinutes(5)
                ? cacheRequest.AbsoluteExpirationRelativeToNow
                : TimeSpan.FromMinutes(_cacheOptions.ExpirationTimeInMinute) != TimeSpan.FromMinutes(5)
                    ? TimeSpan.FromMinutes(_cacheOptions.ExpirationTimeInMinute)
                    : cacheRequest.AbsoluteExpirationRelativeToNow;

        await foreach (var response in next().WithCancellation(cancellationToken))
        {
            _cacheProvider.SetAsync(cacheKey, response, expiredTimeSpan, cancellationToken).GetAwaiter().GetResult();

            _logger.LogDebug(
                "Caching response for {TRequest} with cache key: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey
            );

            yield return response;
        }
    }
}
