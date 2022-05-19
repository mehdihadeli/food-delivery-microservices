namespace BuildingBlocks.Abstractions.CQRS.Events.Internal;

public interface IDomainNotificationEventHandler<in TEvent> : IEventHandler<TEvent>
    where TEvent : IDomainNotificationEvent

{
}
