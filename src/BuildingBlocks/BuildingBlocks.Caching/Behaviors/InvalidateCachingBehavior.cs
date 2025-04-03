using BuildingBlocks.Abstractions.Caching;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Caching.Behaviors;

public class InvalidateCachingBehavior<TRequest, TResponse>(
    ILogger<InvalidateCachingBehavior<TRequest, TResponse>> logger,
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
        if (message is not IInvalidateCacheRequest<TRequest, TResponse> cacheRequest)
        {
            // No cache policy found, so just continue through the pipeline
            return await next(message, cancellationToken);
        }

        var cacheKeys = cacheRequest.CacheKeys(message);
        var response = await next(message, cancellationToken);

        foreach (var cacheKey in cacheKeys)
        {
            await cacheService.RemoveAsync(cacheKey, cancellationToken);
            logger.LogDebug("Cache data with cache key: {CacheKey} invalidated", cacheKey);
        }

        return response;
    }
}

public class StreamInvalidateCachingBehavior<TRequest, TResponse>(
    ILogger<StreamInvalidateCachingBehavior<TRequest, TResponse>> logger,
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
        if (message is not IStreamInvalidateCacheRequest<TRequest, TResponse> cacheRequest)
        {
            // If the request does not implement IStreamCacheRequest, go to the next pipeline
            await foreach (var response in next(message, cancellationToken))
            {
                yield return response;
            }

            yield break;
        }

        await foreach (var response in next(message, cancellationToken))
        {
            var cacheKeys = cacheRequest.CacheKeys(message);

            foreach (var cacheKey in cacheKeys)
            {
                await cacheService.RemoveAsync(cacheKey, cancellationToken);
                logger.LogDebug("Cache data with cache key: {CacheKey} invalidated", cacheKey);
            }

            yield return response;
        }
    }
}
