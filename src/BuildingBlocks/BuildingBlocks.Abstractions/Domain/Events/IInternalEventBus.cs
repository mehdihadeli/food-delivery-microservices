using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Abstractions.Domain.Events;

public interface IInternalEventBus
{
    Task Publish(IEvent @event, CancellationToken ct);
    Task Publish(IMessage @event, CancellationToken ct);

    Task Publish(IStreamEvent @event, CancellationToken ct);
    Task Publish<T>(IStreamEvent<T> @event, CancellationToken ct)
        where T : IDomainEvent;
}
