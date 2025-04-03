using System.Collections.Concurrent;
using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using Polly.Wrap;

namespace BuildingBlocks.Core.Messages;

public class MessagePublisher(AsyncPolicyWrap policy, IServiceProvider serviceProvider) : IMessagePublisher
{
    private readonly ConcurrentDictionary<Type, MessageHandler> _messageHandlerCache = new();
    private readonly ConcurrentDictionary<Type, MessageEnvelopeHandler> _messageEnvelopeHandlerCache = new();
    private readonly ConcurrentDictionary<Type, StreamEventEnvelopeHandler> _streamEventEnvelopeHandlerCache = new();

    public async Task Publish<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageType = typeof(TMessage);

        // Get or create the handler delegate for the message type
        var handler = _messageHandlerCache.GetOrAdd(
            messageType,
            type =>
            {
                var handlers = serviceProvider.GetServices<IMessageHandler<TMessage>>().ToList();

                if (handlers.Count == 0)
                {
                    // No handlers found
                    return (_, _) => Task.CompletedTask;
                }

                // Combine all handlers into a single delegate
                return async (m, ct) =>
                {
                    var tasks = handlers.Select(h => h.Handle((TMessage)m, ct));
                    await Task.WhenAll(tasks);
                };
            }
        );

        // Invoke the handler with the policy
        await policy.ExecuteAsync(() => handler(message, cancellationToken));
    }

    public async Task Publish<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(messageEnvelope);

        var messageType = typeof(TMessage);

        // Get or create the handler delegate for the message envelope type
        var handler = _messageEnvelopeHandlerCache.GetOrAdd(
            messageType,
            type =>
            {
                var handlers = serviceProvider.GetServices<IMessageEnvelopeHandler<TMessage>>().ToList();

                if (handlers.Count == 0)
                {
                    // No handlers found
                    return (_, _) => Task.CompletedTask;
                }

                // Combine all handlers into a single delegate
                return async (envelope, ct) =>
                {
                    var tasks = handlers.Select(h => h.Handle((IMessageEnvelope<TMessage>)envelope, ct));
                    await Task.WhenAll(tasks);
                };
            }
        );

        // Invoke the handler with the policy
        await policy.ExecuteAsync(() => handler(messageEnvelope, cancellationToken));
    }

    public async Task Publish<TStreamEvent>(
        IStreamEventEnvelope<TStreamEvent> streamEventEnvelope,
        CancellationToken cancellationToken = default
    )
        where TStreamEvent : class, IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(streamEventEnvelope);

        var eventType = typeof(TStreamEvent);

        // Get or create the handler delegate for the stream event type
        var handler = _streamEventEnvelopeHandlerCache.GetOrAdd(
            eventType,
            type =>
            {
                var handlers = serviceProvider.GetServices<IStreamEnvelopeHandler<TStreamEvent>>().ToList();

                if (handlers.Count == 0)
                {
                    // No handlers found
                    return (_, _) => Task.CompletedTask;
                }

                // Combine all handlers into a single delegate
                return async (envelope, ct) =>
                {
                    var tasks = handlers.Select(h => h.Handle((IStreamEventEnvelope<TStreamEvent>)envelope, ct));
                    await Task.WhenAll(tasks);
                };
            }
        );

        // Invoke the handler with the policy
        await policy.ExecuteAsync(() => handler(streamEventEnvelope, cancellationToken));
    }
}

public delegate Task MessageHandler(IMessage message, CancellationToken cancellationToken);

public delegate Task MessageEnvelopeHandler(
    IMessageEnvelope<IMessage> messageEnvelope,
    CancellationToken cancellationToken
);

public delegate Task StreamEventEnvelopeHandler(
    IStreamEventEnvelope<IDomainEvent> messageEnvelope,
    CancellationToken cancellationToken
);
