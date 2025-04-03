namespace BuildingBlocks.Abstractions.Events;

public interface IDomainEventHandler<in TEvent> : IEventHandler<TEvent>
    where TEvent : IDomainEvent;
