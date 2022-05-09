using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Domain.EventSourcing;
using BuildingBlocks.Abstractions.Persistence.EventStore.Projections;

namespace BuildingBlocks.Abstractions.Persistence.EventStore;

public interface IHaveEventSourcingAggregate :
    IHaveAggregateStateProjection,
    IHaveAggregate,
    IHaveEventSourcedAggregateVersion
{
    /// <summary>
    /// Loads the current state of the aggregate from a list of events.
    /// </summary>
    /// <param name="history">Domain events from the aggregate stream.</param>
    void LoadFromHistory(IEnumerable<IDomainEvent> history);
}
