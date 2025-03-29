namespace BuildingBlocks.Abstractions.Events;

public interface IDomainNotificationEventHandler<in TNotificationEvent, TDomainEvent>
    : IEventHandler<TNotificationEvent>
    where TNotificationEvent : IDomainNotificationEvent<TDomainEvent>
    where TDomainEvent : IDomainEvent;
