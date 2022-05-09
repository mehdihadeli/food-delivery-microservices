using BuildingBlocks.Abstractions.CQRS.Event;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IIntegrationEventHandler<in TEvent> : IEventHandler<TEvent>
    where TEvent : IIntegrationEvent
{
}
