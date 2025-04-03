using BuildingBlocks.Abstractions.Domain;

namespace BuildingBlocks.Abstractions.Events;

public interface IAggregatesDomainEventsRequestStorage
{
    /// <summary>
    /// Add domain events to the aggregate store and remove uncommitted events from aggregate.
    /// </summary>
    /// <param name="aggregate"></param>
    /// <typeparam name="TAggregate"></typeparam>
    /// <returns></returns>
    IReadOnlyList<IDomainEvent> AddEventsFromAggregate<TAggregate>(TAggregate aggregate)
        where TAggregate : IAggregateBase;

    void AddEvents(IReadOnlyList<IDomainEvent> events);

    IReadOnlyList<IDomainEvent> GetAllUncommittedEvents();
    IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents();

    void ClearDomainEvents();
}
