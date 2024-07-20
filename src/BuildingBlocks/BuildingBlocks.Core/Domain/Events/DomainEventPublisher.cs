using System.Collections.Immutable;
using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Domain.Events;

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IMessagePersistenceService _messagePersistenceService;
    private readonly IDomainEventsAccessor _domainEventsAccessor;
    private readonly IInternalEventBus _internalEventBus;
    private readonly ILogger<DomainEventPublisher> _logger;
    private readonly IEnumerable<IIntegrationEventMapper>? _integrationEventMappers;
    private readonly IEnumerable<IIDomainNotificationEventMapper>? _domainNotificationEventMappers;
    private readonly IEnumerable<IEventMapper>? _eventMappers;
    private readonly IDomainNotificationEventPublisher _domainNotificationEventPublisher;

    public DomainEventPublisher(
        IMessagePersistenceService messagePersistenceService,
        IDomainNotificationEventPublisher domainNotificationEventPublisher,
        IDomainEventsAccessor domainEventsAccessor,
        IInternalEventBus internalEventBus,
        ILogger<DomainEventPublisher> logger,
        IEnumerable<IIntegrationEventMapper>? integrationEventMappers = null,
        IEnumerable<IIDomainNotificationEventMapper>? domainNotificationEventMappers = null,
        IEnumerable<IEventMapper>? eventMappers = null
    )
    {
        _messagePersistenceService = messagePersistenceService;
        _domainEventsAccessor = domainEventsAccessor;
        _internalEventBus = internalEventBus;
        _logger = logger;
        _integrationEventMappers = integrationEventMappers;
        _domainNotificationEventMappers = domainNotificationEventMappers;
        _eventMappers = eventMappers;
        _domainNotificationEventPublisher = domainNotificationEventPublisher;
    }

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return PublishAsync(new[] { domainEvent }, cancellationToken);
    }

    public async Task PublishAsync(IDomainEvent[] domainEvents, CancellationToken cancellationToken = default)
    {
        domainEvents.NotBeNull();

        if (!domainEvents.Any())
            return;

        // https://github.com/dotnet-architecture/eShopOnContainers/issues/700#issuecomment-461807560
        // https://github.com/dotnet-architecture/eShopOnContainers/blob/e05a87658128106fef4e628ccb830bc89325d9da/src/Services/Ordering/Ordering.Infrastructure/OrderingContext.cs#L65
        // http://www.kamilgrzybek.com/design/how-to-publish-and-handle-domain-events/
        // http://www.kamilgrzybek.com/design/handling-domain-events-missing-part/
        // https://www.ledjonbehluli.com/posts/domain_to_integration_event/

        // Dispatch our domain events before commit
        var eventsToDispatch = domainEvents.ToList();

        if (!eventsToDispatch.Any())
        {
            eventsToDispatch = new List<IDomainEvent>(_domainEventsAccessor.UnCommittedDomainEvents);
        }

        await DispatchAsync(eventsToDispatch.ToArray(), cancellationToken);

        // Save wrapped integration and notification events to outbox for further processing after commit
        var wrappedNotificationEvents = eventsToDispatch.GetWrappedDomainNotificationEvents().ToArray();
        await _domainNotificationEventPublisher.PublishAsync(wrappedNotificationEvents.ToArray(), cancellationToken);

        var wrappedIntegrationEvents = eventsToDispatch.GetWrappedIntegrationEvents().ToArray();
        foreach (var wrappedIntegrationEvent in wrappedIntegrationEvents)
        {
            await _messagePersistenceService.AddPublishMessageAsync(
                new MessageEnvelope(wrappedIntegrationEvent, new Dictionary<string, object?>()),
                cancellationToken
            );
        }

        // Save event mapper events into outbox for further processing after commit
        var integrationEvents = GetIntegrationEvents(eventsToDispatch);
        if (integrationEvents.Any())
        {
            foreach (var integrationEvent in integrationEvents)
            {
                await _messagePersistenceService.AddPublishMessageAsync(
                    new MessageEnvelope(integrationEvent, new Dictionary<string, object?>()),
                    cancellationToken
                );
            }
        }

        var notificationEvents = GetNotificationEvents(eventsToDispatch);

        if (notificationEvents.Any())
        {
            foreach (var notification in notificationEvents)
            {
                await _messagePersistenceService.AddNotificationAsync(notification, cancellationToken);
            }
        }
    }

    private IReadOnlyList<IDomainNotificationEvent> GetNotificationEvents(IList<IDomainEvent> eventsToDispatch)
    {
        List<IDomainNotificationEvent> notificationEvents = new List<IDomainNotificationEvent>();

        if (_eventMappers is { } && _eventMappers.Any())
        {
            foreach (var eventMapper in _eventMappers)
            {
                var items = eventMapper.MapToDomainNotificationEvents(eventsToDispatch.AsReadOnly())?.ToList();
                if (items is not null && items.Any())
                {
                    notificationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }
        else if (_domainNotificationEventMappers is { } && notificationEvents.Any())
        {
            foreach (var notificationEventMapper in _domainNotificationEventMappers)
            {
                var items = notificationEventMapper
                    .MapToDomainNotificationEvents(eventsToDispatch.AsReadOnly())
                    ?.ToList();
                if (items is not null && items.Any())
                {
                    notificationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }

        return notificationEvents.ToImmutableList();
    }

    private IReadOnlyList<IIntegrationEvent> GetIntegrationEvents(IList<IDomainEvent> eventsToDispatch)
    {
        List<IIntegrationEvent> integrationEvents = new List<IIntegrationEvent>();

        if (_eventMappers is not null && _eventMappers.Any())
        {
            foreach (var eventMapper in _eventMappers)
            {
                var items = eventMapper.MapToIntegrationEvents(eventsToDispatch.AsReadOnly())?.ToList();
                if (items is not null && items.Any())
                {
                    integrationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }
        else if (_integrationEventMappers is { } && _integrationEventMappers.Any())
        {
            foreach (var integrationEventMapper in _integrationEventMappers)
            {
                var items = integrationEventMapper.MapToIntegrationEvents(eventsToDispatch.AsReadOnly())?.ToList();
                if (items is not null && items.Any())
                {
                    integrationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }

        return integrationEvents.ToImmutableList();
    }

    private async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        @event.NotBeNull();

        if (@event is IIntegrationEvent integrationEvent)
        {
            await _internalEventBus.Publish(integrationEvent, cancellationToken);

            _logger.LogDebug(
                "Dispatched integration notification event {IntegrationEventName} with payload {IntegrationEventContent}",
                integrationEvent.GetType().FullName,
                integrationEvent
            );

            return;
        }

        if (@event is IDomainEvent domainEvent)
        {
            await _internalEventBus.Publish(domainEvent, cancellationToken);

            _logger.LogDebug(
                "Dispatched domain event {DomainEventName} with payload {DomainEventContent}",
                domainEvent.GetType().FullName,
                domainEvent
            );

            return;
        }

        if (@event is IDomainNotificationEvent notificationEvent)
        {
            await _internalEventBus.Publish(notificationEvent, cancellationToken);

            _logger.LogDebug(
                "Dispatched domain notification event {DomainNotificationEventName} with payload {DomainNotificationEventContent}",
                notificationEvent.GetType().FullName,
                notificationEvent
            );
            return;
        }

        await _internalEventBus.Publish(@event, cancellationToken);
    }

    private async Task DispatchAsync<TEvent>(TEvent[] events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        foreach (var @event in events)
        {
            await DispatchAsync(@event, cancellationToken);
        }
    }
}
