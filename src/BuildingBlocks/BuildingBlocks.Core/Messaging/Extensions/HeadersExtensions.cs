using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Messaging.Extensions;

public static class HeadersExtensions
{
    public static void AddCorrelationId(this IDictionary<string, object?> header, string correlationId)
        => header.Add(MessageHeaders.CorrelationId, correlationId);

    public static Guid? GetCorrelationId(this IDictionary<string, object?> header)
    {
        var id = header.Get<string>(MessageHeaders.CorrelationId);
        Guid.TryParse(id, out Guid result);

        return result;
    }

    public static void AddMessageName(this IDictionary<string, object?> header, string messageName)
        => header.Add(MessageHeaders.Name, messageName);

    public static string? GetMessageName(this IDictionary<string, object?> header)
        => header.Get<string>(MessageHeaders.Name);

    public static string? GetMessageType(this IDictionary<string, object?> header)
        => header.Get<string>(MessageHeaders.Type);

    public static void AddMessageType(this IDictionary<string, object?> header, string messageType)
        => header.Add(MessageHeaders.Type, messageType);

    public static void AddMessageId(this IDictionary<string, object?> header, string messageId)
        => header.Add(MessageHeaders.MessageId, messageId);

    public static Guid? GetMessageId(this IDictionary<string, object?> header)
    {
        var id = header.Get<string>(MessageHeaders.MessageId);
        Guid.TryParse(id, out Guid result);

        return result;
    }

    public static void AddCausationId(this IDictionary<string, object?> header, string causationId)
        => header.Add(MessageHeaders.CausationId, causationId);

    public static string? GetCausationId(this IDictionary<string, object?> header)
        => header.Get<string>(MessageHeaders.CorrelationId);

    public static long? GetCreatedUnixTime(this IDictionary<string, object?> header)
        => header.Get(MessageHeaders.Created) as long?;

    public static void AddCreatedUnixTime(this IDictionary<string, object?> header, long created)
        => header.Add(MessageHeaders.Created, created);
}
