using BuildingBlocks.Abstractions.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Caching;

public class InvalidateCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<InvalidateCachingBehavior<TRequest, TResponse>> _logger;
    private readonly ICacheProvider _cacheProvider;


    public InvalidateCachingBehavior(
        ILogger<InvalidateCachingBehavior<TRequest, TResponse>> logger,
        ICacheManager cacheManager,
        IEnumerable<IInvalidateCachePolicy<TRequest, TResponse>> invalidateCachingPolicies)
    {
        _logger = logger;
        _cacheProvider = cacheManager.DefaultCacheProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IInvalidateCachePolicy<TRequest, TResponse> cachePolicy)
        {
            return await next();
        }

        var cacheKey = cachePolicy.GetCacheKey(request);
        var response = await next();

        await _cacheProvider.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogDebug("Cache data with cache key: {CacheKey} removed", cacheKey);

        return response;
    }
}
