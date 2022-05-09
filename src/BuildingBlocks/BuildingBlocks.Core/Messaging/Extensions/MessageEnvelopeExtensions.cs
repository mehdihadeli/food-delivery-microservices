using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Messaging.Extensions;

public static class MessageEnvelopeExtensions
{
    public static Guid GetMessageId(this MessageEnvelope message)
    {
        var id = message.Headers.Get<string>(MessageHeaders.MessageId);
        if (!string.IsNullOrEmpty(id))
        {
            return Guid.Parse(id);
        }

        return Guid.Empty;
    }

    public static string? GetName(this MessageEnvelope message)
    {
        return message.Headers.Get<string>(MessageHeaders.Name);
    }

    public static string? GetMessageType(this MessageEnvelope message)
    {
        return message.Headers.Get<string>(MessageHeaders.Type);
    }

    public static Guid GetCorrelationId(this MessageEnvelope message)
    {
        var id = message.Headers.Get<string>(MessageHeaders.CorrelationId);
        if (!string.IsNullOrEmpty(id))
        {
            return Guid.Parse(id);
        }

        return Guid.Empty;
    }

    public static string? GetCausationId(this MessageEnvelope message)
    {
        return message.Headers.Get<string>(MessageHeaders.CausationId);
    }

    public static long? GetCreatedUnixTime(this MessageEnvelope message)
    {
        return message.Headers.Get(MessageHeaders.Created) as long?;
    }
}
