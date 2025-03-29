using BuildingBlocks.Abstractions.Caching;
using StackExchange.Redis;

namespace BuildingBlocks.Caching;

public class RedisPubSubService : IRedisPubSubService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisPubSubService"/> class.
    /// </summary>
    /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
    public RedisPubSubService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    /// <summary>
    /// Publishes a message to a Redis channel.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="channelName">The name of the channel.</param>
    /// <param name="message">The message to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PublishAsync<T>(string channelName, T message)
    {
        var database = _connectionMultiplexer.GetDatabase();
        await database.PublishMessage(channelName, message);
    }

    /// <summary>
    /// Publishes a message to a Redis channel using a default channel name based on the message type.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PublishAsync<T>(T message)
    {
        var database = _connectionMultiplexer.GetDatabase();
        await database.PublishMessage(message);
    }

    /// <summary>
    /// Subscribes to a Redis channel and handles incoming messages.
    /// </summary>
    /// <typeparam name="T">The type of the message to handle.</typeparam>
    /// <param name="channelName">The name of the channel.</param>
    /// <param name="handler">The handler function to process incoming messages.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SubscribeAsync<T>(string channelName, Func<string, T, Task> handler)
    {
        var database = _connectionMultiplexer.GetDatabase();
        await database.SubscribeMessage(channelName, handler);
    }

    /// <summary>
    /// Subscribes to a Redis channel and handles incoming messages.
    /// The default channel name is based on the message type.
    /// </summary>
    /// <typeparam name="T">The type of the message to handle.</typeparam>
    /// <param name="handler">The handler function to process incoming messages.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SubscribeAsync<T>(Func<string, T, Task> handler)
    {
        var database = _connectionMultiplexer.GetDatabase();
        await database.SubscribeMessage(handler);
    }

    /// <summary>
    /// Subscribes to a Redis channel and handles incoming messages.
    /// The default channel name is based on the message type.
    /// </summary>
    /// <typeparam name="T">The type of the message to handle.</typeparam>
    /// <param name="handler">The handler function to process incoming messages, including channel name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SubscribeWithChannelAsync<T>(Func<string, T, Task> handler)
    {
        var database = _connectionMultiplexer.GetDatabase();
        await database.SubscribeMessage(handler);
    }
}
