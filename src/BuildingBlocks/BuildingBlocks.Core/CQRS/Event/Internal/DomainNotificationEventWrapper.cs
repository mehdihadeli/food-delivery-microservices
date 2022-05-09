using BuildingBlocks.Abstractions.CQRS.Event.Internal;

namespace BuildingBlocks.Core.CQRS.Event.Internal;

public record DomainNotificationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent) : DomainNotificationEvent
    where TDomainEventType : IDomainEvent;
