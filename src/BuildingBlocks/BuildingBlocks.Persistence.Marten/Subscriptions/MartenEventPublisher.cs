using Marten;
using Marten.Events;
using MediatR;

namespace BuildingBlocks.Persistence.Marten.Subscriptions;

public class MartenEventPublisher(IMediator mediator) : IMartenEventsConsumer
{
    public async Task ConsumeAsync(
        IDocumentOperations documentOperations,
        IReadOnlyList<StreamAction> streamActions,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
        {
            await mediator.Publish(@event, cancellationToken);
        }
    }
}
