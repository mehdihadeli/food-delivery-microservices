using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Core.Extensions;
using EasyCaching.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Caching.Behaviours;

public class InvalidateCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly ILogger<InvalidateCachingBehavior<TRequest, TResponse>> _logger;
    private readonly IEasyCachingProvider _cacheProvider;

    public InvalidateCachingBehavior(
        ILogger<InvalidateCachingBehavior<TRequest, TResponse>> logger,
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<CacheOptions> cacheOptions
    )
    {
        _logger = logger;
        _cacheProvider = cachingProviderFactory.GetCachingProvider(cacheOptions.Value.DefaultCacheType);
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not IInvalidateCacheRequest<TRequest, TResponse> cacheRequest)
        {
            // No cache policy found, so just continue through the pipeline
            return await next();
        }

        var cacheKeys = cacheRequest.CacheKeys(request);
        var response = await next();

        foreach (var cacheKey in cacheKeys)
        {
            await _cacheProvider.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Cache data with cache key: {CacheKey} invalidated", cacheKey);
        }

        return response;
    }
}

public class StreamInvalidateCachingBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TResponse : class
{
    private readonly ILogger<StreamInvalidateCachingBehavior<TRequest, TResponse>> _logger;
    private readonly IEasyCachingProvider _cacheProvider;

    public StreamInvalidateCachingBehavior(
        ILogger<StreamInvalidateCachingBehavior<TRequest, TResponse>> logger,
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<CacheOptions> cacheOptions
    )
    {
        _logger = logger;
        _cacheProvider = cachingProviderFactory.GetCachingProvider(cacheOptions.Value.DefaultCacheType);
    }

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not IStreamInvalidateCacheRequest<TRequest, TResponse> cacheRequest)
        {
            // If the request does not implement IStreamCacheRequest, go to the next pipeline
            await foreach (var response in next().WithCancellation(cancellationToken))
            {
                yield return response;
            }

            yield break;
        }

        await foreach (var response in next().WithCancellation(cancellationToken))
        {
            var cacheKeys = cacheRequest.CacheKeys(request);

            foreach (var cacheKey in cacheKeys)
            {
                await _cacheProvider.RemoveAsync(cacheKey, cancellationToken);
                _logger.LogDebug("Cache data with cache key: {CacheKey} invalidated", cacheKey);
            }

            yield return response;
        }
    }
}
