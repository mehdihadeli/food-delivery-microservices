using BuildingBlocks.Abstractions.CQRS.Event.Internal;

namespace BuildingBlocks.Core.CQRS.Event.Internal;

public abstract record DomainNotificationEvent : Event, IDomainNotificationEvent;
