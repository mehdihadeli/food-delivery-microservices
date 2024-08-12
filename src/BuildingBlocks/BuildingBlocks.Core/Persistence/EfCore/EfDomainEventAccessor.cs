using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Persistence.EfCore;

public class EfDomainEventAccessor(
    IDomainEventContext domainEventContext,
    IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore
) : IDomainEventsAccessor
{
    public IReadOnlyList<IDomainEvent> UnCommittedDomainEvents
    {
        get
        {
            _ = aggregatesDomainEventsStore.GetAllUncommittedEvents();

            // Or
            return domainEventContext.GetAllUncommittedEvents();
        }
    }
}
