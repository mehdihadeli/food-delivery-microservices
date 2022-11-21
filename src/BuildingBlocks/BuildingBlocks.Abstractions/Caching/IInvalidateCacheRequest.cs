using MediatR;

namespace BuildingBlocks.Abstractions.Caching;

public interface IInvalidateCacheRequest<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    string Prefix { get; }
    IEnumerable<string> CacheKeys(TRequest request);
}

public interface IInvalidateCacheRequest<in TRequest> : IInvalidateCacheRequest<TRequest, Unit>
    where TRequest : IRequest<Unit>
{
}
