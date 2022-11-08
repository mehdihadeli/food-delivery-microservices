using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Core.Types;
using Humanizer;
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
        IDictionary<string, object?> meta = headers ?? new Dictionary<string, object?>();
        meta = GetMetadata(message, meta);

        var envelope = new MessageEnvelope<TMessage>(message, meta);

        await _publishEndpoint.Publish(message, ctx =>
            {
                foreach (var header in meta)
                {
                    ctx.Headers.Set(header.Key, header.Value);
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
        IDictionary<string, object?> meta = headers ?? new Dictionary<string, object?>();
        meta = GetMetadata(message, meta);

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
                foreach (var header in meta)
                {
                    ctx.Headers.Set(header.Key, header.Value);
                }
            },
            cancellationToken);
    }

    public async Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers,
        CancellationToken cancellationToken = default)
    {
        IDictionary<string, object?> meta = headers ?? new Dictionary<string, object?>();
        if (message is IMessage data)
        {
            meta = GetMetadata(data, meta);
        }
        else
        {
            meta = GetMetadata(message, meta);
        }

        await _publishEndpoint.Publish(message, ctx =>
            {
                foreach (var header in meta)
                {
                    ctx.Headers.Set(header.Key, header.Value);
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

        IDictionary<string, object?> meta = headers ?? new Dictionary<string, object?>();
        if (message is IMessage data)
        {
            meta = GetMetadata(data, meta);
        }
        else
        {
            meta = GetMetadata(message, meta);
        }

        // Ref: https://stackoverflow.com/a/60269493/581476
        string endpointAddress = GetEndpointAddress(exchangeOrTopic, queue);

        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri(endpointAddress));
        await sendEndpoint.Send(message, ctx =>
            {
                foreach (var header in meta)
                {
                    ctx.Headers.Set(header.Key, header.Value);
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

    public void Consume<TMessage>(
        IMessageHandler<TMessage> handler,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null)
        where TMessage : class, IMessage
    {
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

    private static IDictionary<string, object?> GetMetadata<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers)
        where TMessage : class, IMessage
    {
        var meta = headers ?? new Dictionary<string, object?>();

        if (!meta.ContainsKey(MessageHeaders.MessageId))
        {
            //TODO: Using snowflake id here
            var messageId = message.MessageId;
            meta.AddMessageId(messageId.ToString());
        }

        if (!meta.ContainsKey(MessageHeaders.CorrelationId))
        {
            meta.AddCorrelationId(Guid.NewGuid().ToString());
        }

        meta.AddMessageName(message.GetType().Name.Underscore());
        meta.AddMessageType(TypeMapper.GetTypeName(message.GetType()));
        meta.AddCreatedUnixTime(DateTimeExtensions.ToUnixTimeSecond(DateTime.Now));
        return meta;
    }

    private static IDictionary<string, object?> GetMetadata(object message, IDictionary<string, object?>? headers)
    {
        var meta = headers ?? new Dictionary<string, object?>();

        if (!meta.ContainsKey(MessageHeaders.MessageId))
        {
            //TODO: Using snowflake id here
            var messageId = Guid.NewGuid();
            meta.AddMessageId(messageId.ToString());
        }

        if (!meta.ContainsKey(MessageHeaders.CorrelationId))
        {
            meta.AddCorrelationId(Guid.NewGuid().ToString());
        }

        meta.AddMessageName(message.GetType().Name.Underscore());
        meta.AddMessageType(TypeMapper.GetTypeName(message.GetType())); // out of process message should have just type name instead of type full name
        meta.AddCreatedUnixTime(DateTimeExtensions.ToUnixTimeSecond(DateTime.Now));
        return meta;
    }
}
