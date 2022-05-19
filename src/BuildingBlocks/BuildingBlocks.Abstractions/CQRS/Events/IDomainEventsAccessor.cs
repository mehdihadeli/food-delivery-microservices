using BuildingBlocks.Abstractions.CQRS.Events.Internal;

namespace BuildingBlocks.Abstractions.CQRS.Events;

public interface IDomainEventsAccessor
{
    IReadOnlyList<IDomainEvent> UnCommittedDomainEvents { get; }
}
