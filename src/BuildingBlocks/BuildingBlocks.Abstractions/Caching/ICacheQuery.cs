using BuildingBlocks.Abstractions.Queries;

namespace BuildingBlocks.Abstractions.Caching;

public interface ICacheQuery<in TRequest, TResponse> : IQuery<TResponse>, ICacheRequest<TRequest, TResponse>
    where TResponse : class
    where TRequest : IQuery<TResponse>;
