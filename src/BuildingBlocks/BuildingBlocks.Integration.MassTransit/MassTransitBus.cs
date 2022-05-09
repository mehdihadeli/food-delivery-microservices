using BuildingBlocks.Abstractions.Messaging;
using MassTransit;
using IBus = BuildingBlocks.Abstractions.Messaging.IBus;

namespace BuildingBlocks.Integration.MassTransit;

public class MassTransitBus : IBus
{
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitBus(ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
    {
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        await _publishEndpoint.Publish(message, ctx =>
            {
                if (headers is { })
                {
                    foreach (var header in headers)
                    {
                        ctx.Headers.Set(header.Key, header.Value);
                    }
                }
            },
            cancellationToken);
    }

    public async Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        if (string.IsNullOrEmpty(queue) && string.IsNullOrEmpty(exchangeOrTopic))
        {
            await PublishAsync(message, headers, cancellationToken);
            return;
        }

        // Ref: https://stackoverflow.com/a/60269493/581476
        string endpointAddress = GetEndpointAddress(exchangeOrTopic, queue);

        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri(endpointAddress));
        await sendEndpoint.Send(message, ctx =>
            {
                if (headers is { })
                {
                    foreach (var header in headers)
                    {
                        ctx.Headers.Set(header.Key, header.Value);
                    }
                }
            },
            cancellationToken);
    }

    public async Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers,
        CancellationToken cancellationToken = default)
    {
        await _publishEndpoint.Publish(message, ctx =>
            {
                if (headers is { })
                {
                    foreach (var header in headers)
                    {
                        ctx.Headers.Set(header.Key, header.Value);
                    }
                }
            },
            cancellationToken);
    }

    public async Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(queue) && string.IsNullOrEmpty(exchangeOrTopic))
        {
            await PublishAsync(message, headers, cancellationToken);
            return;
        }

        // Ref: https://stackoverflow.com/a/60269493/581476
        string endpointAddress = GetEndpointAddress(exchangeOrTopic, queue);

        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri(endpointAddress));
        await sendEndpoint.Send(message, ctx =>
            {
                if (headers is { })
                {
                    foreach (var header in headers)
                    {
                        ctx.Headers.Set(header.Key, header.Value);
                    }
                }
            },
            cancellationToken);
    }

    private static string GetEndpointAddress(string? exchangeOrTopic, string? queue)
    {
        return !string.IsNullOrEmpty(queue) && !string.IsNullOrEmpty(exchangeOrTopic)
            ? $"exchange:{exchangeOrTopic}?bind=true&queue={queue}"
            : !string.IsNullOrEmpty(queue)
                ? $"queue={queue}"
                : $"exchange:{exchangeOrTopic}";
    }

    public Task Consume<TMessage>(
        Abstractions.Messaging.MessageHandler<TMessage> subscribeMethod,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task Consume<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task Consume(Type messageType, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Consume<THandler, TMessage>(CancellationToken cancellationToken = default)
        where THandler : IMessageHandler<TMessage>
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task ConsumeAll(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ConsumeAllFromAssemblyOf<TType>(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ConsumeAllFromAssemblyOf(
        CancellationToken cancellationToken = default,
        params Type[] assemblyMarkerTypes)
    {
        return Task.CompletedTask;
    }
}
