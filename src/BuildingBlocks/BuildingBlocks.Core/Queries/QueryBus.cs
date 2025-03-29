namespace BuildingBlocks.Core.Queries;

using BuildingBlocks.Abstractions.Queries;
using Mediator;

public class QueryBus(IMediator mediator) : IQueryBus
{
    public ValueTask<TResponse> SendAsync<TResponse>(
        Abstractions.Queries.IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
        where TResponse : notnull
    {
        return mediator.Send(query, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> SendAsync<TResponse>(
        Abstractions.Queries.IStreamQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
        where TResponse : notnull
    {
        return mediator.CreateStream(query, cancellationToken);
    }
}
