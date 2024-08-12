using BuildingBlocks.Abstractions.Queries;
using MediatR;

namespace BuildingBlocks.Core.Queries;

public class QueryBus(IMediator mediator) : IQueryBus
{
    public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        where TResponse : notnull
    {
        return mediator.Send(query, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> SendAsync<TResponse>(
        IStreamQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
        where TResponse : notnull
    {
        return mediator.CreateStream(query, cancellationToken);
    }
}
