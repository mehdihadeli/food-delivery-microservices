using BuildingBlocks.Abstractions.Domain;

namespace BuildingBlocks.Abstractions.Events;

public interface IAggregatesDomainEventsRequestStorage
{
    IReadOnlyList<IDomainEvent> AddEventsFromAggregate<T>(T aggregate)
        where T : IHaveAggregate;

    void AddEvents(IReadOnlyList<IDomainEvent> events);

    IReadOnlyList<IDomainEvent> GetAllUncommittedEvents();
}
