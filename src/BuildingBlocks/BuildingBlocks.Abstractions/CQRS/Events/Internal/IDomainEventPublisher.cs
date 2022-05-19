namespace BuildingBlocks.Abstractions.CQRS.Events.Internal;

public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task PublishAsync(IDomainEvent[] domainEvents, CancellationToken cancellationToken = default);
}
