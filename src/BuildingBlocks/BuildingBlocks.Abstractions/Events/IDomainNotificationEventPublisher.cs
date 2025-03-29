namespace BuildingBlocks.Abstractions.Events;

public interface IDomainNotificationEventPublisher
{
    Task PublishAsync<T>(
        IDomainNotificationEvent<T> domainNotificationEvent,
        CancellationToken cancellationToken = default
    )
        where T : class, IDomainEvent;

    Task PublishAsync<T>(
        IEnumerable<IDomainNotificationEvent<T>> domainNotificationEvents,
        CancellationToken cancellationToken = default
    )
        where T : class, IDomainEvent;
}
