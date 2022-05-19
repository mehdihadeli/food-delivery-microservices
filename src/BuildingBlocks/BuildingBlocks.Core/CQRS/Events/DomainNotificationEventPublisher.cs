using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;

namespace BuildingBlocks.Core.CQRS.Events;

public class DomainNotificationEventPublisher : IDomainNotificationEventPublisher
{
    private readonly IMessagePersistenceService _messagePersistenceService;

    public DomainNotificationEventPublisher(IMessagePersistenceService messagePersistenceService)
    {
        _messagePersistenceService = messagePersistenceService;
    }

    public Task PublishAsync(
        IDomainNotificationEvent domainNotificationEvent,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvent, nameof(domainNotificationEvent));

        return _messagePersistenceService.AddNotificationAsync(domainNotificationEvent, cancellationToken);
    }

    public async Task PublishAsync(
        IDomainNotificationEvent[] domainNotificationEvents,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvents, nameof(domainNotificationEvents));

        foreach (var domainNotificationEvent in domainNotificationEvents)
        {
            await _messagePersistenceService.AddNotificationAsync(domainNotificationEvent, cancellationToken);
        }
    }
}
