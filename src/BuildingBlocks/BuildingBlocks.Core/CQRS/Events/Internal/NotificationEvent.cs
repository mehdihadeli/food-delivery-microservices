namespace BuildingBlocks.Core.CQRS.Events.Internal;

// Just for executing after transaction
public record NotificationEvent(dynamic Data) : DomainNotificationEvent;
