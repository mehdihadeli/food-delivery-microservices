namespace BuildingBlocks.Abstractions.Caching;

/// <summary>
/// Interface for a Redis-based Pub/Sub messaging service.
/// Provides methods to publish and subscribe to messages on Redis channels.
/// </summary>
public interface IRedisPubSubService
{
    /// <summary>
    /// Publishes a message to a specified Redis channel.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="channelName">The name of the channel to publish the message to.</param>
    /// <param name="message">The message to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<T>(string channelName, T message);

    /// <summary>
    /// Publishes a message to a Redis channel.
    /// The channel name is automatically derived from the type of the message.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<T>(T message);

    /// <summary>
    /// Subscribes to a specified Redis channel and processes incoming messages.
    /// </summary>
    /// <typeparam name="T">The type of the messages to process.</typeparam>
    /// <param name="channelName">The name of the channel to subscribe to.</param>
    /// <param name="handler">A function that processes messages, providing the channel name and message content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SubscribeAsync<T>(string channelName, Func<string, T, Task> handler);

    /// <summary>
    /// Subscribes to a Redis channel and processes incoming messages.
    /// The channel name is automatically derived from the type of the message.
    /// </summary>
    /// <typeparam name="T">The type of the messages to process.</typeparam>
    /// <param name="handler">A function that processes messages.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SubscribeAsync<T>(Func<string, T, Task> handler);

    /// <summary>
    /// Subscribes to a Redis channel and processes incoming messages.
    /// The channel name is automatically derived from the type of the message.
    /// The handler receives both the channel name and message content.
    /// </summary>
    /// <typeparam name="T">The type of the messages to process.</typeparam>
    /// <param name="handler">A function that processes messages, including the channel name and message content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SubscribeWithChannelAsync<T>(Func<string, T, Task> handler);
}
