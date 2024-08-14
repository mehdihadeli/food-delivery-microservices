using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Core.Messaging;

public class NullExternalEventBus : IExternalEventBus
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync(IEventEnvelope eventEnvelope, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync(
        IEventEnvelope eventEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }

    public void Consume<TMessage>(
        IMessageHandler<TMessage> handler,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null
    )
        where TMessage : IMessage { }

    public Task Consume<TMessage>(
        MessageHandler<TMessage> subscribeMethod,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : IMessage
    {
        return Task.CompletedTask;
    }

    public Task Consume<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        return Task.CompletedTask;
    }

    public Task Consume(Type messageType, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Consume<THandler, TMessage>(CancellationToken cancellationToken = default)
        where THandler : IMessageHandler<TMessage>
        where TMessage : IMessage
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
        params Type[] assemblyMarkerTypes
    )
    {
        return Task.CompletedTask;
    }
}
