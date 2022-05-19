using BuildingBlocks.Abstractions.CQRS.Events;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IIntegrationEventHandler<in TEvent> : IEventHandler<TEvent>
    where TEvent : IIntegrationEvent
{
}
