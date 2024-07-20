using BuildingBlocks.Abstractions.Domain.Events.Internal;

namespace BuildingBlocks.Core.Domain.Events.Internal;

public record DomainNotificationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent) : DomainNotificationEvent
    where TDomainEventType : IDomainEvent;
