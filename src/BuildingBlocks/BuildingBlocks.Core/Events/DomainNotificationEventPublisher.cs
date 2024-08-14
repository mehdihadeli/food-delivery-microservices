using BuildingBlocks.Abstractions.Events.Internal;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Events;

public class DomainNotificationEventPublisher(IMessagePersistenceService messagePersistenceService)
    : IDomainNotificationEventPublisher
{
    public Task PublishAsync(
        IDomainNotificationEvent domainNotificationEvent,
        CancellationToken cancellationToken = default
    )
    {
        domainNotificationEvent.NotBeNull();
        return messagePersistenceService.AddNotificationAsync(domainNotificationEvent, cancellationToken);
    }

    public async Task PublishAsync(
        IDomainNotificationEvent[] domainNotificationEvents,
        CancellationToken cancellationToken = default
    )
    {
        domainNotificationEvents.NotBeNull();

        foreach (var domainNotificationEvent in domainNotificationEvents)
        {
            await messagePersistenceService.AddNotificationAsync(domainNotificationEvent, cancellationToken);
        }
    }
}
