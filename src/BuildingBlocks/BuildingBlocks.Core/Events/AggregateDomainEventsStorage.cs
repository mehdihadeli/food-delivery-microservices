using System.Collections.Immutable;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Events;

public class AggregatesDomainEventsStorage : IAggregatesDomainEventsRequestStorage
{
    private readonly List<IDomainEvent> _uncommittedDomainEvents = new();

    public IReadOnlyList<IDomainEvent> AddEventsFromAggregate<T>(T aggregate)
        where T : IAggregateBase
    {
        var events = aggregate.DequeueUncommittedDomainEvents();

        AddEvents(events);

        return events;
    }

    public void AddEvents(IReadOnlyList<IDomainEvent> events)
    {
        if (events.Any())
        {
            _uncommittedDomainEvents.AddRange(events);
        }
    }

    public IReadOnlyList<IDomainEvent> GetAllUncommittedEvents()
    {
        return _uncommittedDomainEvents;
    }

    public IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents()
    {
        // create a copy because after clearing events we lost our collection
        var events = new List<IDomainEvent>(GetAllUncommittedEvents());
        ClearDomainEvents();

        return events.ToImmutableList();
    }

    public void ClearDomainEvents()
    {
        _uncommittedDomainEvents.Clear();
    }
}
