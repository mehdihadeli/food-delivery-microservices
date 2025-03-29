using System.Collections.Concurrent;
using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using Mediator;
using Polly;
using Polly.Wrap;

namespace BuildingBlocks.Core.Events;

public class InternalEventBus(IMediator mediator, AsyncPolicyWrap policy) : IInternalEventBus
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _publishMethods = new();

    public async Task Publish<T>(T eventData, CancellationToken ct)
        where T : class, IEvent
    {
        var retryAsync = Policy.Handle<System.Exception>().RetryAsync(2);

        await retryAsync
            .ExecuteAsync(async c => await mediator.Publish(eventData, c).ConfigureAwait(false), ct)
            .ConfigureAwait(false);
    }

    public async Task Publish<T>(IEnumerable<T> eventsData, CancellationToken ct)
        where T : class, IEvent
    {
        foreach (var eventData in eventsData)
        {
            await Publish(eventData, ct).ConfigureAwait(false);
        }
    }

    public async Task Publish<T>(IMessageEnvelope<T> messageEnvelope, CancellationToken ct)
        where T : class, Abstractions.Messages.IMessage
    {
        await policy
            .ExecuteAsync(async c => await mediator.Publish(messageEnvelope, c).ConfigureAwait(false), ct)
            .ConfigureAwait(false);
    }

    public Task Publish(IMessageEnvelopeBase messageEnvelope, CancellationToken ct)
    {
        // calling generic `Publish<T>` in `InternalEventBus` class
        var genericPublishMethod = _publishMethods.GetOrAdd(
            messageEnvelope.Message.GetType(),
            eventType =>
                typeof(InternalEventBus)
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .First(m => m.Name == nameof(Publish) && m.GetGenericArguments().Length != 0)
                    .MakeGenericMethod(eventType)
        );

        return (Task)genericPublishMethod.Invoke(this, new object[] { messageEnvelope, ct })!;
    }

    public async Task Publish<T>(IStreamEventEnvelope<T> streamEvent, CancellationToken ct)
        where T : class, IDomainEvent
    {
        await policy
            .ExecuteAsync(async c => await mediator.Publish(streamEvent, c).ConfigureAwait(false), ct)
            .ConfigureAwait(false);
    }

    public Task Publish(IStreamEventEnvelopeBase streamEvent, CancellationToken ct)
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
