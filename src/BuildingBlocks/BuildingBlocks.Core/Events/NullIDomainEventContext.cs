using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Events;

public class NullIDomainEventContext : IDomainEventContext
{
    public IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents()
    {
        return new List<IDomainEvent>();
    }
}
