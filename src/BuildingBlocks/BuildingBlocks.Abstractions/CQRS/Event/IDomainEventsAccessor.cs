using BuildingBlocks.Abstractions.CQRS.Event.Internal;

namespace BuildingBlocks.Abstractions.CQRS.Event;

public interface IDomainEventsAccessor
{
    IReadOnlyList<IDomainEvent> UnCommittedDomainEvents { get; }
}
