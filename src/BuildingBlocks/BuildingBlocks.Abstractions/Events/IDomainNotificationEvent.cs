namespace BuildingBlocks.Abstractions.Events;

public interface IDomainNotificationEvent<out TDomainEvent> : IEvent
    where TDomainEvent : IDomainEvent
{
    TDomainEvent DomainEvent { get; }
}

//public interface IDomainNotificationEvent : IEvent;
