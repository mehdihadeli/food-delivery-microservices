using Mediator;

namespace BuildingBlocks.Abstractions.Caching;

public interface IInvalidateCacheRequest<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    string Prefix { get; }
    IEnumerable<string> CacheKeys(TRequest request);
}

public interface IInvalidateCacheRequest<in TRequest> : IInvalidateCacheRequest<TRequest, Unit>
    where TRequest : IRequest<Unit>;

public interface IStreamInvalidateCacheRequest<in TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    string Prefix { get; }
    IEnumerable<string> CacheKeys(TRequest request);
}
