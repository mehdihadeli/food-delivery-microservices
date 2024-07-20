using BuildingBlocks.Abstractions.Domain.Events.Internal;

namespace BuildingBlocks.Abstractions.Domain.Events;

public interface IAggregatesDomainEventsRequestStore
{
    IReadOnlyList<IDomainEvent> AddEventsFromAggregate<T>(T aggregate)
        where T : IHaveAggregate;

    void AddEvents(IReadOnlyList<IDomainEvent> events);

    IReadOnlyList<IDomainEvent> GetAllUncommittedEvents();
}
