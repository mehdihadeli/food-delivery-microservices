using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Core.Messages;

public class NullIBusDirectPublisher : IBusDirectPublisher
{
    public Task PublishAsync<TMessage>(
        IMessageEnvelope<TMessage> messageEnvelope,
        CancellationToken cancellationToken = default
    )
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync(IMessageEnvelopeBase messageEnvelope, CancellationToken cancellationToken = default)
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

    public Task PublishAsync(
        IMessageEnvelopeBase messageEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }
}
