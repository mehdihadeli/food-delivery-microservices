using BuildingBlocks.Abstractions.Domain.Events.Internal;

namespace BuildingBlocks.Core.Domain.Events.Internal;

public abstract record DomainNotificationEvent : Event, IDomainNotificationEvent;
