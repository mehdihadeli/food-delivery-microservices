using BuildingBlocks.Abstractions.Events;

namespace BuildingBlocks.Abstractions.Messaging;

public interface IBusProducer
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage;

    public Task PublishAsync(IEventEnvelope eventEnvelope, CancellationToken cancellationToken = default);

    public Task PublishAsync(
        IEventEnvelope eventEnvelope,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default
    );
}
