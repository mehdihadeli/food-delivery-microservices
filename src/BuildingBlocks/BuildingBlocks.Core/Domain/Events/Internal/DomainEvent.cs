using BuildingBlocks.Abstractions.Domain.Events.Internal;

namespace BuildingBlocks.Core.Domain.Events.Internal;

public record DomainEvent : Event, IDomainEvent
{
    public dynamic AggregateId { get; private set; } = default!;
    public long AggregateSequenceNumber { get; private set; }

    public virtual IDomainEvent WithAggregate(dynamic aggregateId, long version)
    {
        AggregateId = aggregateId;
        AggregateSequenceNumber = version;

        return this;
    }
}
