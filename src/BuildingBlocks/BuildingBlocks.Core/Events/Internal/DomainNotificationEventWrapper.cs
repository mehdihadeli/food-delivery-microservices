using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Events.Internal;

public abstract record DomainNotificationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent)
    : DomainNotificationEvent<TDomainEventType>(DomainEvent)
    where TDomainEventType : IDomainEvent;
