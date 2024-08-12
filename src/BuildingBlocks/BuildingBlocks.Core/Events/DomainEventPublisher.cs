using System.Collections.Immutable;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Events.Extensions;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Events;

public class DomainEventPublisher(
    IMessagePersistenceService messagePersistenceService,
    IDomainNotificationEventPublisher domainNotificationEventPublisher,
    IDomainEventsAccessor domainEventsAccessor,
    IInternalEventBus internalEventBus,
    IMessageMetadataAccessor messageMetadataAccessor,
    IServiceProvider serviceProvider
) : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return PublishAsync([domainEvent,], cancellationToken);
    }

    public async Task PublishAsync(IDomainEvent[] domainEvents, CancellationToken cancellationToken = default)
    {
        domainEvents.NotBeNull();

        if (domainEvents.Length == 0)
            return;

        // https://github.com/dotnet-architecture/eShopOnContainers/issues/700#issuecomment-461807560
        // https://github.com/dotnet-architecture/eShopOnContainers/blob/e05a87658128106fef4e628ccb830bc89325d9da/src/Services/Ordering/Ordering.Infrastructure/OrderingContext.cs#L65
        // http://www.kamilgrzybek.com/design/how-to-publish-and-handle-domain-events/
        // http://www.kamilgrzybek.com/design/handling-domain-events-missing-part/
        // https://www.ledjonbehluli.com/posts/domain_to_integration_event/

        // Dispatch our domain events before commit
        var eventsToDispatch = domainEvents.ToList();

        if (eventsToDispatch.Count == 0)
        {
            eventsToDispatch = [.. domainEventsAccessor.UnCommittedDomainEvents,];
        }

        // Dispatch events to internal broker
        await internalEventBus.Publish(eventsToDispatch, cancellationToken);

        // Save wrapped integration and notification events to outbox for further processing after commit
        var wrappedNotificationEvents = eventsToDispatch.GetWrappedDomainNotificationEvents().ToArray();
        await domainNotificationEventPublisher.PublishAsync(wrappedNotificationEvents.ToArray(), cancellationToken);

        var wrappedIntegrationEvents = eventsToDispatch.GetWrappedIntegrationEvents().ToArray();
        foreach (var wrappedIntegrationEvent in wrappedIntegrationEvents)
        {
            var correlationId = messageMetadataAccessor.GetCorrelationId();
            var cautionId = messageMetadataAccessor.GetCautionId();
            var eventEnvelope = EventEnvelope.From(wrappedIntegrationEvent, correlationId, cautionId);
            await messagePersistenceService.AddPublishMessageAsync(eventEnvelope, cancellationToken);
        }

        var eventMappers = serviceProvider.GetServices<IEventMapper>().ToImmutableList();

        // Save event mapper events into outbox for further processing after commit
        var integrationEvents = GetIntegrationEvents(serviceProvider, eventMappers, eventsToDispatch);
        if (!integrationEvents.IsEmpty)
        {
            foreach (var integrationEvent in integrationEvents)
            {
                var correlationId = messageMetadataAccessor.GetCorrelationId();
                var cautionId = messageMetadataAccessor.GetCautionId();
                var eventEnvelope = EventEnvelope.From(integrationEvent, correlationId, cautionId);
                await messagePersistenceService.AddPublishMessageAsync(eventEnvelope, cancellationToken);
            }
        }

        var notificationEvents = GetNotificationEvents(serviceProvider, eventMappers, eventsToDispatch);

        if (!notificationEvents.IsEmpty)
        {
            foreach (var notification in notificationEvents)
            {
                await messagePersistenceService.AddNotificationAsync(notification, cancellationToken);
            }
        }
    }

    private static ImmutableList<IDomainNotificationEvent> GetNotificationEvents(
        IServiceProvider serviceProvider,
        IReadOnlyList<IEventMapper> eventMappers,
        IReadOnlyList<IDomainEvent> eventsToDispatch
    )
    {
        var notificationEventMappers = serviceProvider.GetServices<IIDomainNotificationEventMapper>().ToImmutableList();

        List<IDomainNotificationEvent> notificationEvents = new List<IDomainNotificationEvent>();

        if (eventMappers.Any())
        {
            foreach (var eventMapper in eventMappers)
            {
                var items = eventMapper.MapToDomainNotificationEvents(eventsToDispatch)?.ToList();
                if (items is not null && items.Count != 0)
                {
                    notificationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }
        else if (!notificationEventMappers.IsEmpty)
        {
            foreach (var notificationEventMapper in notificationEventMappers)
            {
                var items = notificationEventMapper.MapToDomainNotificationEvents(eventsToDispatch)?.ToList();
                if (items is not null && items.Count != 0)
                {
                    notificationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }

        return notificationEvents.ToImmutableList();
    }

    private static ImmutableList<IIntegrationEvent> GetIntegrationEvents(
        IServiceProvider serviceProvider,
        IReadOnlyList<IEventMapper> eventMappers,
        IReadOnlyList<IDomainEvent> eventsToDispatch
    )
    {
        var integrationEventMappers = serviceProvider.GetServices<IIntegrationEventMapper>().ToImmutableList();

        List<IIntegrationEvent> integrationEvents = new List<IIntegrationEvent>();

        if (eventMappers.Any())
        {
            foreach (var eventMapper in eventMappers)
            {
                var items = eventMapper.MapToIntegrationEvents(eventsToDispatch)?.ToList();
                if (items is not null && items.Count != 0)
                {
                    integrationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }
        else if (!integrationEventMappers.IsEmpty)
        {
            foreach (var integrationEventMapper in integrationEventMappers)
            {
                var items = integrationEventMapper.MapToIntegrationEvents(eventsToDispatch)?.ToList();
                if (items is not null && items.Count != 0)
                {
                    integrationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }

        return integrationEvents.ToImmutableList();
    }
}
