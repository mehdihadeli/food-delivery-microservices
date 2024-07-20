using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Domain.Events.Internal;

namespace BuildingBlocks.Persistence.EventStoreDB;

public class EventStoreDomainEventAccessor : IDomainEventsAccessor
{
    private readonly IAggregatesDomainEventsRequestStore _aggregatesDomainEventsStore;

    public EventStoreDomainEventAccessor(IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
    {
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
    }

    public IReadOnlyList<IDomainEvent> UnCommittedDomainEvents =>
        _aggregatesDomainEventsStore.GetAllUncommittedEvents();
}
