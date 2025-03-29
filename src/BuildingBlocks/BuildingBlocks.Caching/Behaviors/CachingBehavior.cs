using BuildingBlocks.Abstractions.Caching;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Caching.Behaviors;

// Ref: https://anderly.com/2019/12/12/cross-cutting-concerns-with-mediatr-pipeline-behaviors/
public class CachingBehavior<TRequest, TResponse>(
    ILogger<CachingBehavior<TRequest, TResponse>> logger,
    ICacheService cacheService
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TRequest, TResponse> next
    )
    {
        if (message is not ICacheRequest<TRequest, TResponse> cacheRequest)
        {
            // No cache policy found, so just continue through the pipeline
            return await next(message, cancellationToken);
        }

        var cacheKey = cacheRequest.CacheKey(message);
        var cachedResponse = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            logger.LogDebug(
                "Response retrieved {TRequest} from cache. CacheKey: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey
            );

            return cachedResponse;
        }

        var response = await next(message, cancellationToken);

        await cacheService.SetAsync(
            key: cacheKey,
            value: response,
            localExpiration: cacheRequest.AbsoluteLocalCacheExpirationRelativeToNow,
            distributedExpiration: cacheRequest.AbsoluteExpirationRelativeToNow,
            cancellationToken: cancellationToken
        );

        logger.LogDebug(
            "Caching response for {TRequest} with cache key: {CacheKey}",
            typeof(TRequest).FullName,
            cacheKey
        );

        return response;
    }
}

public class StreamCachingBehavior<TRequest, TResponse>(
    ILogger<StreamCachingBehavior<TRequest, TResponse>> logger,
    ICacheService cacheService
) : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TResponse : class
{
    public async IAsyncEnumerable<TResponse> Handle(
        TRequest message,
        CancellationToken cancellationToken,
        StreamHandlerDelegate<TRequest, TResponse> next
    )
    {
        if (message is not IStreamCacheRequest<TRequest, TResponse> cacheRequest)
        {
            // If the request does not implement IStreamCacheRequest, go to the next pipeline
            await foreach (var response in next(message, cancellationToken))
            {
                yield return response;
            }

            yield break;
        }

        var cacheKey = cacheRequest.CacheKey(message);
        var cachedResponse = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            logger.LogDebug(
                "Response retrieved {TRequest} from cache. CacheKey: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey
            );

            yield return cachedResponse;
            yield break;
        }

        await foreach (var response in next(message, cancellationToken))
        {
            await cacheService.SetAsync(
                key: cacheKey,
                value: response,
                localExpiration: cacheRequest.AbsoluteLocalCacheExpirationRelativeToNow,
                distributedExpiration: cacheRequest.AbsoluteExpirationRelativeToNow,
                cancellationToken: cancellationToken
            );

            logger.LogDebug(
                "Caching response for {TRequest} with cache key: {CacheKey}",
                typeof(TRequest).FullName,
                cacheKey
            );

            yield return response;
        }
    }
}
