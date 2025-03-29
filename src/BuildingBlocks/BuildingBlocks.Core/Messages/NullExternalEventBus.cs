namespace BuildingBlocks.Core.Messages;

using BuildingBlocks.Abstractions.Messages;

public class NullExternalEventBus : IExternalEventBus
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync(IMessageEnvelopeBase messageEnvelope, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(
        TMessage message,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }
}
