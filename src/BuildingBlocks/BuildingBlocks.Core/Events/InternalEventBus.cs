using System.Collections.Concurrent;
using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using MediatR;
using Polly;

namespace BuildingBlocks.Core.Events;

public class InternalEventBus(IMediator mediator, AsyncPolicy policy) : IInternalEventBus
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _publishMethods = new();

    public async Task Publish(IEvent @event, CancellationToken ct)
    {
        var retryAsync = Policy.Handle<System.Exception>().RetryAsync(2);

        await retryAsync.ExecuteAsync(c => mediator.Publish(@event, c), ct);
    }

    public async Task Publish(IEnumerable<IEvent> eventsData, CancellationToken ct)
    {
        foreach (var eventData in eventsData)
        {
            await Publish(eventData, ct).ConfigureAwait(false);
        }
    }

    public async Task Publish<T>(IEventEnvelope<T> eventEnvelope, CancellationToken ct)
        where T : class
    {
        await policy.ExecuteAsync(
            c =>
            {
                // TODO: using metadata for tracing ang monitoring here
                return mediator.Publish(eventEnvelope.Message, c);
            },
            ct
        );
    }

    public Task Publish(IEventEnvelope eventEnvelope, CancellationToken ct)
    {
        // calling generic `Publish<T>` in `InternalEventBus` class
        var genericPublishMethod = _publishMethods.GetOrAdd(
            eventEnvelope.Message.GetType(),
            eventType =>
                typeof(InternalEventBus)
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .First(m => m.Name == nameof(Publish) && m.GetGenericArguments().Length != 0)
                    .MakeGenericMethod(eventType)
        );

        return (Task)genericPublishMethod.Invoke(this, new object[] { eventEnvelope, ct })!;
    }

    public async Task Publish<T>(IStreamEventEnvelope<T> streamEvent, CancellationToken ct)
        where T : IDomainEvent
    {
        await policy.ExecuteAsync(
            c =>
            {
                // TODO: using metadata for tracing ang monitoring here
                return mediator.Publish(streamEvent.Data, c);
            },
            ct
        );
    }

    public Task Publish(IStreamEventEnvelope streamEvent, CancellationToken ct)
    {
        // calling generic `Publish<T>` in `InternalEventBus` class
        var genericPublishMethod = _publishMethods.GetOrAdd(
            streamEvent.Data.GetType(),
            eventType =>
                typeof(InternalEventBus)
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .First(m => m.Name == nameof(Publish) && m.GetGenericArguments().Length != 0)
                    .MakeGenericMethod(eventType)
        );

        return (Task)genericPublishMethod.Invoke(this, new object[] { streamEvent, ct })!;
    }
}
