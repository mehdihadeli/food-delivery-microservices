using BuildingBlocks.Abstractions.CQRS.Event.Internal;

namespace BuildingBlocks.Core.CQRS.Event.Internal;

public abstract record DomainEvent : Event, IDomainEvent
{
    public dynamic AggregateId { get; protected set; } = null!;
    public long AggregateSequenceNumber { get; protected set; }

    public virtual IDomainEvent WithAggregate(dynamic aggregateId, long version)
    {
        AggregateId = aggregateId;
        AggregateSequenceNumber = version;

        return this;
    }
}
