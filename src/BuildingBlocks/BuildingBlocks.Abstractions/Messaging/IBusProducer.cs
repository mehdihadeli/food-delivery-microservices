namespace BuildingBlocks.Abstractions.Messaging;

public interface IBusProducer
{
    public Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;

    public Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;

    public Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers,
        CancellationToken cancellationToken = default);

    public Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default);
}
