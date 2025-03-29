namespace BuildingBlocks.Caching;

using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

public static class RedisPubSubExtensions
{
    public static async Task PublishMessage<T>(this IDatabase database, string channelName, T data)
    {
        var jsonData = JsonConvert.SerializeObject(
            data,
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
        );

        await database.PublishAsync(channelName, jsonData);
    }

    public static async Task PublishMessage<T>(this IDatabase database, T data)
    {
        var channelName = $"{typeof(T).Name.Underscore()}_channel";
        await database.PublishMessage(channelName, data);
    }

    public static async Task PublishMessage<T>(this ITransaction transaction, string channelName, T data)
    {
        var jsonData = JsonConvert.SerializeObject(
            data,
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
        );

        await transaction.PublishAsync(channelName, jsonData);
    }

    public static async Task PublishMessage<T>(this ITransaction transaction, T data)
    {
        var channelName = $"{typeof(T).Name.Underscore()}_channel";
        await transaction.PublishMessage(channelName, data);
    }

    public static async Task SubscribeMessage<T>(
        this IDatabase database,
        string channelName,
        Func<string, T, Task> handler
    )
    {
        var channelMessageQueue = await database.Multiplexer.GetSubscriber().SubscribeAsync(channelName);

        channelMessageQueue.OnMessage(async channelMessage =>
        {
            var message = JsonConvert.DeserializeObject<T>(channelMessage.Message!);
            await handler(channelMessage.Channel!, message!);
        });
    }

    public static async Task SubscribeMessage<T>(this IDatabase database, string channelName, Func<T, Task> handler)
    {
        var channelMessageQueue = await database.Multiplexer.GetSubscriber().SubscribeAsync(channelName);

        channelMessageQueue.OnMessage(async channelMessage =>
        {
            var message = JsonConvert.DeserializeObject<T>(channelMessage.Message!);
            await handler(message!);
        });
    }

    public static async Task SubscribeMessage<T>(this IDatabase database, Func<string, T, Task> handler)
    {
        var channelName = $"{typeof(T).Name.Underscore()}_channel";

        await database.SubscribeMessage(channelName, handler);
    }
}
