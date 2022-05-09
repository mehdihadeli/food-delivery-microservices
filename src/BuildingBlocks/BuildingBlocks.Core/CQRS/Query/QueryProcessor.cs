using BuildingBlocks.Abstractions.CQRS.Query;
using MediatR;

namespace BuildingBlocks.Core.CQRS.Query;

public class QueryProcessor : IQueryProcessor
{
    private readonly IMediator _mediator;

    public QueryProcessor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        return _mediator.Send(query, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> SendAsync<TResponse>(
        IStreamQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        return _mediator.CreateStream(query, cancellationToken);
    }
}
