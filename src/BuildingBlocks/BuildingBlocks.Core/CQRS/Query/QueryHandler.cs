using BuildingBlocks.Abstractions.CQRS.Query;

namespace BuildingBlocks.Core.CQRS.Query;

public abstract class QueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
    protected abstract Task<TResponse> HandleQueryAsync(TQuery query, CancellationToken cancellationToken = default);

    public Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken)
    {
        return HandleQueryAsync(request, cancellationToken);
    }
}
