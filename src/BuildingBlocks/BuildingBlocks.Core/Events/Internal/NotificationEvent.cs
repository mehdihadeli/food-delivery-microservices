namespace BuildingBlocks.Core.Events.Internal;

// Just for executing after transaction
public record NotificationEvent(dynamic Data) : DomainNotificationEvent;
