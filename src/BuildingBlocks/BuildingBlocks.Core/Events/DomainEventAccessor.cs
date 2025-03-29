using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Events;

public class DomainEventAccessor(
    IAggregatesDomainEventsRequestStorage aggregatesDomainEventsStorage,
    IDomainEventContext domainEventContext
) : IDomainEventsAccessor
{
    public IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents()
    {
        // works based on `AggregatesDomainEventsRequestStorageInterceptor`
        var events = aggregatesDomainEventsStorage.DequeueUncommittedDomainEvents();
        if (events.Count != 0)
        {
            return events;
        }

        // Or
        return domainEventContext.DequeueUncommittedDomainEvents();
    }
}
