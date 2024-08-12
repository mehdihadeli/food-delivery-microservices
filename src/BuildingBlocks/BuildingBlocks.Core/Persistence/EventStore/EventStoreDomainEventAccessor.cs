using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Persistence.EventStore;

public abstract class EventStoreDomainEventAccessor(IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
    : IDomainEventsAccessor
{
    public IReadOnlyList<IDomainEvent> UnCommittedDomainEvents => aggregatesDomainEventsStore.GetAllUncommittedEvents();
}
