namespace BuildingBlocks.Abstractions.CQRS.Event.Internal;

public interface IDomainNotificationEvent<TDomainEventType> : IDomainNotificationEvent
    where TDomainEventType : IDomainEvent
{
    TDomainEventType DomainEvent { get; set; }
}

public interface IDomainNotificationEvent : IEvent
{
}
