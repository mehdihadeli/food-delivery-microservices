using BuildingBlocks.Abstractions.Persistence.EventStore;

namespace BuildingBlocks.Abstractions.Events;

public interface IInternalEventBus
{
    /// <summary>
    ///     publish an in-memory event.
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish(IEvent eventData, CancellationToken ct);

    /// <summary>
    ///     publish multiple in-memory events.
    /// </summary>
    /// <param name="eventsData"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish(IEnumerable<IEvent> eventsData, CancellationToken ct);

    /// <summary>
    ///     publish an in-memory event based on consumed event from messaging system.
    /// </summary>
    /// <param name="eventEnvelope"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish(IEventEnvelope eventEnvelope, CancellationToken ct);

    /// <summary>
    ///     publish an in-memory event based on consumed event from messaging system.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventEnvelope"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish<T>(IEventEnvelope<T> eventEnvelope, CancellationToken ct)
        where T : class;

    /// <summary>
    ///     publish an in-memory event based consumed events from eventstore.
    /// </summary>
    /// <param name="streamEventEnvelope"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish(IStreamEventEnvelope streamEventEnvelope, CancellationToken ct);

    /// <summary>
    ///     publish an in-memory event based consumed events from eventstore.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="streamEventEnvelope"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Publish<T>(IStreamEventEnvelope<T> streamEventEnvelope, CancellationToken ct)
        where T : IDomainEvent;
}
