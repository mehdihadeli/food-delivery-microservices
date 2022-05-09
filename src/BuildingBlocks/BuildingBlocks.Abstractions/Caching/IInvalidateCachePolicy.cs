using MediatR;

namespace BuildingBlocks.Abstractions.Caching;

public interface IInvalidateCachePolicy<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    string GetCacheKey(TRequest request)
    {
        var r = new { request };
        var props = r.request.GetType().GetProperties().Select(pi => $"{pi.Name}:{pi.GetValue(r.request, null)}");
        return $"{typeof(TRequest).FullName}{{{string.Join(",", props)}}}";
    }
}

public interface IInvalidateCachePolicy<in TRequest> : IInvalidateCachePolicy<TRequest, Unit>
    where TRequest : IRequest<Unit>
{
}
