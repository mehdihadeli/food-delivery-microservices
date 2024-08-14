using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Abstractions.Queries;

namespace BuildingBlocks.Caching;

public abstract record CacheQuery<TRequest, TResponse> : ICacheQuery<TRequest, TResponse>
    where TRequest : IQuery<TResponse>
    where TResponse : class
{
    public virtual TimeSpan AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);

    // public virtual TimeSpan SlidingExpiration => TimeSpan.FromSeconds(30);
    // public virtual DateTime? AbsoluteExpiration => null;
    public virtual string Prefix => "Ch_";

    public virtual string CacheKey(TRequest request) => $"{Prefix}{typeof(TRequest).Name}";
}
