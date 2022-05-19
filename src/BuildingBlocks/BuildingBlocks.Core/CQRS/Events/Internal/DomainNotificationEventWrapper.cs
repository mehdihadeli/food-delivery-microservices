using BuildingBlocks.Abstractions.CQRS.Events.Internal;

namespace BuildingBlocks.Core.CQRS.Events.Internal;

public record DomainNotificationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent) : DomainNotificationEvent
    where TDomainEventType : IDomainEvent;
