using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Abstractions.Domain.EventSourcing;

public interface IEventSourcedAggregate<out TId> : IEntity<TId>, IHaveEventSourcingAggregate
{
}

public interface IEventSourcedAggregate<out TIdentity, TId> : IEventSourcedAggregate<TIdentity>
    where TIdentity : Identity<TId>
{
}

public interface IEventSourcedAggregate : IEventSourcedAggregate<AggregateId, long>
{
}
