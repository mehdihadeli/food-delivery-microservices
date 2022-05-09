using BuildingBlocks.Abstractions.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Caching;

public class InvalidateCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly IEnumerable<IInvalidateCachePolicy<TRequest, TResponse>> _invalidateCachePolicies;
    private readonly ILogger<InvalidateCachingBehavior<TRequest, TResponse>> _logger;
    private readonly ICacheManager _cacheManager;


    public InvalidateCachingBehavior(
        ILogger<InvalidateCachingBehavior<TRequest, TResponse>> logger,
        ICacheManager cacheManager,
        IEnumerable<IInvalidateCachePolicy<TRequest, TResponse>> invalidateCachingPolicies)
    {
        _logger = logger;
        _cacheManager = cacheManager;
        _invalidateCachePolicies = invalidateCachingPolicies;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var cachePolicy = _invalidateCachePolicies.FirstOrDefault();
        if (cachePolicy == null)
        {
            // No cache policy found, so just continue through the pipeline
            return await next();
        }

        var cacheKey = cachePolicy.GetCacheKey(request);
        var response = await next();

        await _cacheManager.RemoveAsync(cacheKey);

        _logger.LogDebug("Cache data with cache key: {CacheKey} removed", cacheKey);

        return response;
    }
}
