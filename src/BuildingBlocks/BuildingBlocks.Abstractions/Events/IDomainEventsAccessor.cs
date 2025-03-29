namespace BuildingBlocks.Abstractions.Events;

public interface IDomainEventsAccessor
{
    IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents();
}
