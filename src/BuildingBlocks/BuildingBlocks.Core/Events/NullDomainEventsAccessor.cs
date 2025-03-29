using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Events;

public class NullDomainEventsAccessor : IDomainEventsAccessor
{
    public IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents()
    {
        return new List<IDomainEvent>();
    }
}
