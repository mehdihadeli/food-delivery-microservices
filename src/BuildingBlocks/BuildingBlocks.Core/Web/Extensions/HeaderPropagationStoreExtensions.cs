using BuildingBlocks.Core.Messages;
using Microsoft.AspNetCore.HeaderPropagation;

namespace BuildingBlocks.Core.Web.HeaderPropagation.Extensions;

public static class HeaderPropagationStoreExtensions
{
    public static void AddCorrelationId(this HeaderPropagationValues headerPropagationStore, Guid correlationId)
    {
        if (
            headerPropagationStore.Headers is not null
            && !headerPropagationStore.Headers.ContainsKey(MessageHeaders.CorrelationId)
        )
        {
            headerPropagationStore.Headers.Add(MessageHeaders.CorrelationId, correlationId.ToString());
        }
    }

    public static Guid? GetCorrelationId(this HeaderPropagationValues headerPropagationStore)
    {
        if (headerPropagationStore.Headers is null)
            return null;

        headerPropagationStore.Headers.TryGetValue(MessageHeaders.CorrelationId, out var cid);

        return string.IsNullOrEmpty(cid) ? null : Guid.Parse(cid!);
    }

    public static void AddMessageName(this HeaderPropagationValues headerPropagationStore, string messageName)
    {
        if (headerPropagationStore.Headers is null)
            return;

        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.CorrelationId))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.Name, messageName);
        }
    }

    public static string? GetMessageName(this HeaderPropagationValues headerPropagationStore)
    {
        if (headerPropagationStore.Headers is null)
            return null;

        headerPropagationStore.Headers.TryGetValue(MessageHeaders.Name, out var name);

        return name;
    }

    public static string? GetMessageType(this HeaderPropagationValues headerPropagationStore)
    {
        if (headerPropagationStore.Headers is null)
            return null;

        headerPropagationStore.Headers.TryGetValue(MessageHeaders.Type, out var type);

        return type;
    }

    public static void AddMessageType(this HeaderPropagationValues headerPropagationStore, string messageType)
    {
        if (headerPropagationStore.Headers is null)
            return;

        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.Type))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.Type, messageType);
        }
    }

    public static void AddMessageId(this HeaderPropagationValues headerPropagationStore, Guid messageId)
    {
        if (headerPropagationStore.Headers is null)
            return;

        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.MessageId))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.MessageId, messageId.ToString());
        }
    }

    public static Guid? GetMessageId(this HeaderPropagationValues headerPropagationStore)
    {
        if (headerPropagationStore.Headers is null)
            return null;

        headerPropagationStore.Headers.TryGetValue(MessageHeaders.MessageId, out var messageId);

        return string.IsNullOrEmpty(messageId) ? null : Guid.Parse(messageId!);
    }

    public static void AddCausationId(this HeaderPropagationValues headerPropagationStore, Guid causationId)
    {
        if (headerPropagationStore.Headers is null)
            return;

        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.CausationId))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.CausationId, causationId.ToString());
        }
    }

    public static Guid? GetCausationId(this HeaderPropagationValues headerPropagationStore)
    {
        if (headerPropagationStore.Headers is null)
            return null;

        headerPropagationStore.Headers.TryGetValue(MessageHeaders.CausationId, out var cautionId);

        return string.IsNullOrEmpty(cautionId) ? null : Guid.Parse(cautionId!);
    }

    public static long? GetCreatedUnixTime(this HeaderPropagationValues headerPropagationStore)
    {
        if (headerPropagationStore.Headers is null)
            return null;

        headerPropagationStore.Headers.TryGetValue(MessageHeaders.Created, out var created);

        return string.IsNullOrEmpty(created) ? null : long.Parse(created);
    }

    public static void AddCreatedUnixTime(this HeaderPropagationValues headerPropagationStore, long created)
    {
        if (headerPropagationStore.Headers is null)
            return;

        if (!headerPropagationStore.Headers.ContainsKey(MessageHeaders.Created))
        {
            headerPropagationStore.Headers.Add(MessageHeaders.Created, created.ToString());
        }
    }
}
