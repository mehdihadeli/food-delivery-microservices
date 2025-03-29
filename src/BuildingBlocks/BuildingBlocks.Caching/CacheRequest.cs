using BuildingBlocks.Abstractions.Caching;
using Mediator;

namespace BuildingBlocks.Caching;

public abstract record CacheRequest<TRequest, TResponse> : ICacheRequest<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public virtual TimeSpan? AbsoluteExpirationRelativeToNow { get; }
    public virtual TimeSpan? AbsoluteLocalCacheExpirationRelativeToNow { get; }
    public virtual string Prefix => "Ch_";

    public virtual string CacheKey(TRequest request) => $"{Prefix}{typeof(TRequest).Name}";
}
