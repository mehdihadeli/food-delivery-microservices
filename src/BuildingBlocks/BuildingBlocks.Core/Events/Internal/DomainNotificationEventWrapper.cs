using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Events.Internal;

public record DomainNotificationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent) : DomainNotificationEvent
    where TDomainEventType : IDomainEvent;
