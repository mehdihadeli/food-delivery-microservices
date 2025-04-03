using System.Collections.Immutable;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Core.Events.Extensions;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messages;

namespace BuildingBlocks.Core.Events;

public class DomainEventPublisher(
    IMessagePersistenceService messagePersistenceService,
    IDomainNotificationEventPublisher domainNotificationEventPublisher,
    IInternalEventBus internalEventBus,
    IMessageMetadataAccessor messageMetadataAccessor,
    IServiceProvider serviceProvider
) : IDomainEventPublisher
{
    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
        where T : class, IDomainEvent
    {
        domainEvent.NotBeNull();

        // https://github.com/dotnet-architecture/eShopOnContainers/issues/700#issuecomment-461807560
        // https://github.com/dotnet-architecture/eShopOnContainers/blob/e05a87658128106fef4e628ccb830bc89325d9da/src/Services/Ordering/Ordering.Infrastructure/OrderingContext.cs#L65
        // http://www.kamilgrzybek.com/design/how-to-publish-and-handle-domain-events/
        // http://www.kamilgrzybek.com/design/handling-domain-events-missing-part/
        // https://www.ledjonbehluli.com/posts/domain_to_integration_event/

        // Dispatch events to internal broker
        await internalEventBus.Publish(domainEvent, cancellationToken).ConfigureAwait(false);

        // Save wrapped integration and notification events to outbox for further processing after commit
        var wrappedNotificationEvent = domainEvent.GetWrappedDomainNotificationEvent();
        if (wrappedNotificationEvent != null)
        {
            await domainNotificationEventPublisher
                .PublishAsync(wrappedNotificationEvent, cancellationToken)
                .ConfigureAwait(false);
        }

        var wrappedIntegrationEvent = domainEvent.GetWrappedIntegrationEvent();
        if (wrappedIntegrationEvent != null)
        {
            var correlationId = messageMetadataAccessor.GetCorrelationId();
            var cautionId = wrappedIntegrationEvent.MessageId;
            var eventEnvelope = MessageEnvelopeFactory.From(wrappedIntegrationEvent, correlationId, cautionId);
            await messagePersistenceService
                .AddPublishMessageAsync(eventEnvelope, cancellationToken)
                .ConfigureAwait(false);
        }

        var eventMappers = serviceProvider.GetServices<IEventMapper>().ToImmutableList();

        var integrationEvents = GetIntegrationEvents(serviceProvider, eventMappers, domainEvent);
        if (!integrationEvents.IsEmpty)
        {
            foreach (var integrationEvent in integrationEvents)
            {
                var correlationId = messageMetadataAccessor.GetCorrelationId();
                var cautionId = integrationEvent.MessageId;
                var eventEnvelope = MessageEnvelopeFactory.From(integrationEvent, correlationId, cautionId);
                // Save event mapper events into outbox for further processing after commit
                await messagePersistenceService
                    .AddPublishMessageAsync(eventEnvelope, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        var notificationEvents = GetNotificationEvents(serviceProvider, eventMappers, domainEvent);
        if (!notificationEvents.IsEmpty)
        {
            foreach (var notification in notificationEvents)
            {
                await messagePersistenceService
                    .AddNotificationAsync(notification, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    public async Task PublishAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default)
        where T : class, IDomainEvent
    {
        foreach (var domainEvent in domainEvents)
        {
            await PublishAsync(domainEvent, cancellationToken).ConfigureAwait(false);
        }
    }

    private static ImmutableList<IDomainNotificationEvent<IDomainEvent>> GetNotificationEvents(
        IServiceProvider serviceProvider,
        IReadOnlyList<IEventMapper> eventMappers,
        IDomainEvent domainEvent
    )
    {
        var notificationEventMappers = serviceProvider.GetServices<IDomainNotificationEventMapper>().ToImmutableList();

        List<IDomainNotificationEvent<IDomainEvent>> notificationEvents =
            new List<IDomainNotificationEvent<IDomainEvent>>();

        if (eventMappers.Any())
        {
            foreach (var eventMapper in eventMappers)
            {
                var item = eventMapper.MapToDomainNotificationEvent(domainEvent);
                if (item is not null)
                {
                    notificationEvents.Add(item);
                }
            }
        }
        else if (!notificationEventMappers.IsEmpty)
        {
            foreach (var notificationEventMapper in notificationEventMappers)
            {
                var item = notificationEventMapper.MapToDomainNotificationEvent(domainEvent);
                if (item is not null)
                {
                    notificationEvents.Add(item);
                }
            }
        }

        return notificationEvents.ToImmutableList();
    }

    private static ImmutableList<IIntegrationEvent> GetIntegrationEvents(
        IServiceProvider serviceProvider,
        IReadOnlyList<IEventMapper> eventMappers,
        IDomainEvent domainEvent
    )
    {
        var integrationEventMappers = serviceProvider.GetServices<IIntegrationEventMapper>().ToImmutableList();
        List<IIntegrationEvent> integrationEvents = new List<IIntegrationEvent>();

        if (eventMappers.Any())
        {
            foreach (var eventMapper in eventMappers)
            {
                var item = eventMapper.MapToIntegrationEvent(domainEvent);
                if (item is not null)
                {
                    integrationEvents.Add(item);
                }
            }
        }
        else if (!integrationEventMappers.IsEmpty)
        {
            foreach (var integrationEventMapper in integrationEventMappers)
            {
                var item = integrationEventMapper.MapToIntegrationEvent(domainEvent);
                if (item is not null)
                {
                    integrationEvents.Add(item);
                }
            }
        }

        return integrationEvents.ToImmutableList();
    }
}
