namespace BuildingBlocks.Core.CQRS.Event.Internal;

// Just for executing after transaction
public record NotificationEvent(dynamic Data) : DomainNotificationEvent;
