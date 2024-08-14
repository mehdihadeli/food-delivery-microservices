using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Events;

public class EventHandlerDecorator<TEvent>(IEventHandler<TEvent> eventHandler) : IEventHandler<TEvent>
    where TEvent : IEvent
{
    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Using Activity for tracing
        await eventHandler.Handle(notification, cancellationToken);
    }
}
