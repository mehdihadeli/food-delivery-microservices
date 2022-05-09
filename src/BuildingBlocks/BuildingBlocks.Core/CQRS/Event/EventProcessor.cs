using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Messaging;
using MediatR;
using Serilog;

namespace BuildingBlocks.Core.CQRS.Event;

public class EventProcessor : IEventProcessor
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    public EventProcessor(IMediator mediator, IServiceProvider serviceProvider)
    {
        _mediator = mediator;
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var domainEventPublisher = _serviceProvider.GetRequiredService<IDomainEventPublisher>();
        var domainNotificationEventPublisher = _serviceProvider.GetRequiredService<IDomainNotificationEventPublisher>();
        var integrationEventPublisher = _serviceProvider.GetRequiredService<IBus>();

        if (@event is IIntegrationEvent integrationEvent)
        {
            await integrationEventPublisher.PublishAsync(integrationEvent, null, cancellationToken: cancellationToken);

            return;
        }

        if (@event is IDomainEvent domainEvent)
        {
            await domainEventPublisher.PublishAsync(domainEvent, cancellationToken);

            return;
        }

        if (@event is IDomainNotificationEvent notificationEvent)
        {
            await domainNotificationEventPublisher.PublishAsync(notificationEvent, cancellationToken);
        }
    }

    public async Task PublishAsync<TEvent>(TEvent[] events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }

    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        Guard.Against.Null(@event, nameof(@event));

        if (@event is IIntegrationEvent integrationEvent)
        {
            await _mediator.Publish(integrationEvent, cancellationToken);

            Log.Logger.Debug(
                "Dispatched integration notification event {IntegrationEventName} with payload {IntegrationEventContent}",
                integrationEvent.GetType().FullName,
                integrationEvent);

            return;
        }

        if (@event is IDomainEvent domainEvent)
        {
            await _mediator.Publish(domainEvent, cancellationToken);

            Log.Logger.Debug(
                "Dispatched domain event {DomainEventName} with payload {DomainEventContent}",
                domainEvent.GetType().FullName,
                domainEvent);

            return;
        }

        if (@event is IDomainNotificationEvent notificationEvent)
        {
            await _mediator.Publish(notificationEvent, cancellationToken);

            Log.Logger.Debug(
                "Dispatched domain notification event {DomainNotificationEventName} with payload {DomainNotificationEventContent}",
                notificationEvent.GetType().FullName,
                notificationEvent);
            return;
        }

        await _mediator.Publish(@event, cancellationToken);
    }

    public async Task DispatchAsync<TEvent>(TEvent[] events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        foreach (var @event in events)
        {
            await DispatchAsync(@event, cancellationToken);
        }
    }
}
