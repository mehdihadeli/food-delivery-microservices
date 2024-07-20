using Marten;
using Marten.Events;
using MediatR;

namespace BuildingBlocks.Persistence.Marten.Subscriptions;

public class MartenEventPublisher : IMartenEventsConsumer
{
    private readonly IMediator _mediator;

    public MartenEventPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task ConsumeAsync(
        IDocumentOperations documentOperations,
        IReadOnlyList<StreamAction> streamActions,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
        {
            await _mediator.Publish(@event, cancellationToken);
        }
    }
}
