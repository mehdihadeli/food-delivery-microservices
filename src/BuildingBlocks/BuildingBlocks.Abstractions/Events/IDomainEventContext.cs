namespace BuildingBlocks.Abstractions.Events;

public interface IDomainEventContext
{
    IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents();
}
