using BuildingBlocks.Abstractions.Domain.Events;

namespace BuildingBlocks.Core.Domain.Events;

public class EventHandlerDecorator<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
{
    private readonly IEventHandler<TEvent> _eventHandler;

    public EventHandlerDecorator(IEventHandler<TEvent> eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Using Activity for tracing

        await _eventHandler.Handle(notification, cancellationToken);
    }
}
