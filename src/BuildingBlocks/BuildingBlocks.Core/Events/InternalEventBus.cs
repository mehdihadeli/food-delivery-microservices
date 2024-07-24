using System.Collections.Concurrent;
using System.Reflection;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using MediatR;
using Polly;

namespace BuildingBlocks.Core.Domain.Events;

public class InternalEventBus : IInternalEventBus
{
    private readonly IMediator _mediator;
    private static readonly ConcurrentDictionary<Type, MethodInfo> _publishMethods = new();
    private readonly AsyncPolicy _policy;

    public InternalEventBus(IMediator mediator, AsyncPolicy policy)
    {
        _mediator = mediator;
        _policy = policy;
    }

    public async Task Publish(IEvent @event, CancellationToken ct)
    {
        var policy = Policy.Handle<System.Exception>().RetryAsync(2);

        await policy.ExecuteAsync(c => _mediator.Publish(@event, c), ct);
    }

    public async Task Publish<T>(EventEnvelope<T> eventEnvelope, CancellationToken ct)
        where T : class
    {
        await _policy.ExecuteAsync(
            c =>
            {
                // TODO: using metadata for tracing ang monitoring here
                return _mediator.Publish(eventEnvelope.Data, c);
            },
            ct
        );
    }

    public Task Publish(EventEnvelope eventEnvelope, CancellationToken ct)
    {
        // calling generic `Publish<T>` in `InternalEventBus` class
        var genericPublishMethod = _publishMethods.GetOrAdd(
            eventEnvelope.Data.GetType(),
            eventType =>
                typeof(InternalEventBus)
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Single(m => m.Name == nameof(Publish) && m.GetGenericArguments().Any())
                    .MakeGenericMethod(eventType)
        );

        return (Task)genericPublishMethod.Invoke(this, new object[] { eventEnvelope, ct })!;
    }

    public async Task Publish<T>(IStreamEventEnvelope<T> streamEvent, CancellationToken ct)
        where T : IDomainEvent
    {
        await _policy.ExecuteAsync(
            c =>
            {
                // TODO: using metadata for tracing ang monitoring here
                return _mediator.Publish(streamEvent.Data, c);
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
                    .Single(m => m.Name == nameof(Publish) && m.GetGenericArguments().Any())
                    .MakeGenericMethod(eventType)
        );

        return (Task)genericPublishMethod.Invoke(this, new object[] { streamEvent, ct })!;
    }
}
