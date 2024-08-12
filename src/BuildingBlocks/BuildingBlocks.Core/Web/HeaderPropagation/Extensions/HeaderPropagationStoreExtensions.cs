using BuildingBlocks.Core.Messaging;

namespace BuildingBlocks.Core.Web.HeaderPropagation.Extensions;

public static class HeaderPropagationStoreExtensions
{
    public static void AddCorrelationId(this HeaderPropagationStore headerPropagationStore, Guid correlationId)
    {
        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.CorrelationId))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.CorrelationId, correlationId.ToString());
        }
    }

    public static Guid? GetCorrelationId(this HeaderPropagationStore headerPropagationStore)
    {
        headerPropagationStore.Headers.TryGetValue(MessageHeaders.CorrelationId, out var cid);

        return string.IsNullOrEmpty(cid) ? null : Guid.Parse(cid!);
    }

    public static void AddMessageName(this HeaderPropagationStore headerPropagationStore, string messageName)
    {
        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.CorrelationId))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.Name, messageName);
        }
    }

    public static string? GetMessageName(this HeaderPropagationStore headerPropagationStore)
    {
        headerPropagationStore.Headers.TryGetValue(MessageHeaders.Name, out var name);

        return name;
    }

    public static string? GetMessageType(this HeaderPropagationStore headerPropagationStore)
    {
        headerPropagationStore.Headers.TryGetValue(MessageHeaders.Type, out var type);

        return type;
    }

    public static void AddMessageType(this HeaderPropagationStore headerPropagationStore, string messageType)
    {
        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.Type))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.Type, messageType);
        }
    }

    public static void AddMessageId(this HeaderPropagationStore headerPropagationStore, Guid messageId)
    {
        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.MessageId))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.MessageId, messageId.ToString());
        }
    }

    public static Guid? GetMessageId(this HeaderPropagationStore headerPropagationStore)
    {
        headerPropagationStore.Headers.TryGetValue(MessageHeaders.MessageId, out var messageId);

        return string.IsNullOrEmpty(messageId) ? null : Guid.Parse(messageId!);
    }

    public static void AddCausationId(this HeaderPropagationStore headerPropagationStore, Guid causationId)
    {
        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.CausationId))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.CausationId, causationId.ToString());
        }
    }

    public static Guid? GetCausationId(this HeaderPropagationStore headerPropagationStore)
    {
        headerPropagationStore.Headers.TryGetValue(MessageHeaders.CausationId, out var cautionId);

        return string.IsNullOrEmpty(cautionId) ? null : Guid.Parse(cautionId!);
    }

    public static long? GetCreatedUnixTime(this HeaderPropagationStore headerPropagationStore)
    {
        headerPropagationStore.Headers.TryGetValue(MessageHeaders.Created, out var created);

        return string.IsNullOrEmpty(created) ? null : long.Parse(created);
    }

    public static void AddCreatedUnixTime(this HeaderPropagationStore headerPropagationStore, long created)
    {
        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.Created))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.Created, created.ToString());
        }
    }
}
