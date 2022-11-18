using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Caching;
using EasyCaching.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Caching.Behaviours;

public class InvalidateCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<InvalidateCachingBehavior<TRequest, TResponse>> _logger;
    private readonly IEasyCachingProvider _cacheProvider;
    private readonly IEnumerable<IInvalidateCacheRequest<TRequest, TResponse>> _invalidateCachingPolicies;


    public InvalidateCachingBehavior(
        ILogger<InvalidateCachingBehavior<TRequest, TResponse>> logger,
        IEasyCachingProviderFactory cachingProviderFactory,
        IOptions<CacheOptions> cacheOptions,
        IEnumerable<IInvalidateCacheRequest<TRequest, TResponse>> invalidateCachingPolicies)
    {
        _logger = Guard.Against.Null(logger);
        Guard.Against.Null(cacheOptions.Value);
        _cacheProvider = Guard.Against.Null(cachingProviderFactory).GetCachingProvider(cacheOptions.Value.DefaultCacheType);

        // cachePolicies inject like `FluentValidation` approach as a nested or seperated cache class for commands ,queries
        _invalidateCachingPolicies = invalidateCachingPolicies;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheRequest = _invalidateCachingPolicies.FirstOrDefault();
        if (cacheRequest == null)
        {
            // No cache policy found, so just continue through the pipeline
            return await next();
        }

        var cacheKey = cacheRequest.CacheKey;
        var response = await next();

        await _cacheProvider.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogDebug("Cache data with cache key: {CacheKey} invalidated", cacheKey);

        return response;
    }
}
