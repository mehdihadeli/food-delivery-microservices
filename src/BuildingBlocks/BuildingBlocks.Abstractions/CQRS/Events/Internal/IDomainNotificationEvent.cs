namespace BuildingBlocks.Abstractions.CQRS.Events.Internal;

public interface IDomainNotificationEvent<TDomainEventType> : IDomainNotificationEvent
    where TDomainEventType : IDomainEvent
{
    TDomainEventType DomainEvent { get; set; }
}

public interface IDomainNotificationEvent : IEvent
{
}
