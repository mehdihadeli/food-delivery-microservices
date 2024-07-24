using BuildingBlocks.Abstractions.Events.Internal;

namespace BuildingBlocks.Core.Events.Internal;

public abstract record DomainNotificationEvent : Event, IDomainNotificationEvent;
