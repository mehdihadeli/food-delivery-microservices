using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Events.Internal;

namespace BuildingBlocks.Core.Events;

public class NullDomainEventsAccessor : IDomainEventsAccessor
{
    public IReadOnlyList<IDomainEvent> UnCommittedDomainEvents { get; }
}
