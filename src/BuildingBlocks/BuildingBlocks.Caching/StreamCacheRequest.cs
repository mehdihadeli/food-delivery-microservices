using BuildingBlocks.Abstractions.Caching;
using Mediator;

namespace BuildingBlocks.Caching;

public abstract class StreamCacheRequest<TRequest, TResponse> : IStreamCacheRequest<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    public virtual TimeSpan? AbsoluteExpirationRelativeToNow { get; }
    public virtual TimeSpan? AbsoluteLocalCacheExpirationRelativeToNow { get; }
    public virtual string Prefix => "Ch_";

    public virtual string CacheKey(TRequest request) => $"{Prefix}{typeof(TRequest).Name}";
}
