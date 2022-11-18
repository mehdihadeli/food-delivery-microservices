using MediatR;

namespace BuildingBlocks.Abstractions.Caching;

public interface IInvalidateCacheRequest<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    string Prefix => "Ch_";

    string CacheKey => $"{Prefix}{typeof(TRequest).Name}";
}

public interface IInvalidateCacheRequest<in TRequest> : IInvalidateCacheRequest<TRequest, Unit>
    where TRequest : IRequest<Unit>
{
}
