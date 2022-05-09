using MediatR;

namespace BuildingBlocks.Abstractions.CQRS.Event;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : INotification
{
}
