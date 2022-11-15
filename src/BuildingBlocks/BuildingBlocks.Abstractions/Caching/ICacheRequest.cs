using MediatR;

namespace BuildingBlocks.Abstractions.Caching;

public interface ICacheRequest<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);
    TimeSpan? SlidingExpiration => TimeSpan.FromSeconds(30);
    DateTime? AbsoluteExpiration => null;

    string GetCacheKey(TRequest request)
    {
        var r = new {request};
        var props = r.request.GetType().GetProperties().Select(pi => $"{pi.Name}:{pi.GetValue(r.request, null)}");
        return $"{typeof(TRequest).FullName}{{{string.Join(",", props)}}}";
    }
}

public interface IStreamCacheRequest<in TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);
    TimeSpan? SlidingExpiration => TimeSpan.FromSeconds(30);
    DateTime? AbsoluteExpiration => null;

    string GetCacheKey(TRequest request)
    {
        var r = new {request};
        var props = r.request.GetType().GetProperties().Select(pi => $"{pi.Name}:{pi.GetValue(r.request, null)}");
        return $"{typeof(TRequest).FullName}{{{string.Join(",", props)}}}";
    }
}
