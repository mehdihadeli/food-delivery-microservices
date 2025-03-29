namespace BuildingBlocks.Abstractions.Events;

public interface IDomainEventPublisher
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
        where T : class, IDomainEvent;
    Task PublishAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default)
        where T : class, IDomainEvent;
}
