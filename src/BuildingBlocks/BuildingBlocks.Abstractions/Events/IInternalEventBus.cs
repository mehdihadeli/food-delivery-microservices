using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Abstractions.Events;

public interface IInternalEventBus
{
    /// <summary>
    /// publish an in-memory event.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish(IEvent @event, CancellationToken ct);

    /// <summary>
    /// publish an in-memory event based on consumed event from messaging system.
    /// </summary>
    /// <param name="eventEnvelope"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish(EventEnvelope eventEnvelope, CancellationToken ct);

    /// <summary>
    /// publish an in-memory event based on consumed event from messaging system.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventEnvelope"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish<T>(EventEnvelope<T> eventEnvelope, CancellationToken ct)
        where T : class;

    /// <summary>
    /// publish an in-memory event based consumed events from eventstore.
    /// </summary>
    /// <param name="streamEvent"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish(IEventEnvelope streamEvent, CancellationToken ct);

    /// <summary>
    /// publish an in-memory event based consumed events from eventstore.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="streamEvent"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish<T>(IStreamEventEnvelope<T> streamEvent, CancellationToken ct)
        where T : IDomainEvent;
}
