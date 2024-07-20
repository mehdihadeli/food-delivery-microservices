namespace BuildingBlocks.Abstractions.Domain.Events.Internal;

public interface IDomainEventContext
{
    IReadOnlyList<IDomainEvent> GetAllUncommittedEvents();
    void MarkUncommittedDomainEventAsCommitted();
}
