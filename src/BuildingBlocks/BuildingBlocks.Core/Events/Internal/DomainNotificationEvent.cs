using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Core.Events.Internal;

public abstract record DomainNotificationEvent<TDomainEvent>(TDomainEvent DomainEvent)
    : Event,
        IDomainNotificationEvent<TDomainEvent>
    where TDomainEvent : IDomainEvent;
