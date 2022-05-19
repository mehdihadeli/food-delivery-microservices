using BuildingBlocks.Abstractions.CQRS.Events.Internal;

namespace BuildingBlocks.Core.CQRS.Events.Internal;

public abstract record DomainNotificationEvent : Event, IDomainNotificationEvent;
