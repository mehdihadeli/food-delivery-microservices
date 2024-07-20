namespace BuildingBlocks.Abstractions.Domain.Events.Internal;

public interface IDomainEventHandler<in TEvent> : IEventHandler<TEvent>
    where TEvent : IDomainEvent { }
