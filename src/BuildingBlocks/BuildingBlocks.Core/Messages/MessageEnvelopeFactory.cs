using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core.Types;
using Humanizer;

namespace BuildingBlocks.Core.Messages;

public static class MessageEnvelopeFactory
{
    public static IMessageEnvelope<T> From<T>(T data, MessageEnvelopeMetadata metadata)
        where T : class, IMessage
    {
        return new MessageEnvelope<T>(data, metadata);
    }

    public static IMessageEnvelopeBase From(object data, MessageEnvelopeMetadata metadata)
    {
        var methodInfo = typeof(MessageEnvelopeFactory)
            .GetMethods()
            .FirstOrDefault(x =>
                string.Equals(x.Name, nameof(From), StringComparison.OrdinalIgnoreCase)
                && x.GetGenericArguments().Length != 0
                && x.GetParameters().Length == 2
            );
        var genericMethod = methodInfo.MakeGenericMethod(data.GetType());

        return (IMessageEnvelopeBase)genericMethod.Invoke(null, [data, metadata])!;
    }

    public static IMessageEnvelope<T> From<T>(
        T data,
        Guid correlationId,
        Guid? cautionId = null,
        IDictionary<string, object?>? headers = null
    )
        where T : class, IMessage
    {
        var envelopeMetadata = new MessageEnvelopeMetadata(
            data.MessageId,
            correlationId,
            TypeMapper.AddShortTypeName(data.GetType()),
            data.GetType().Name.Underscore(),
            cautionId
        )
        {
            Headers = headers ?? new Dictionary<string, object?>(),
        };

        return From(data, envelopeMetadata);
    }

    public static IMessageEnvelopeBase From(
        object data,
        Guid correlationId,
        Guid? cautionId = null,
        IDictionary<string, object?>? headers = null
    )
    {
        var methodInfo = typeof(MessageEnvelopeFactory)
            .GetMethods()
            .FirstOrDefault(x =>
                string.Equals(x.Name, nameof(From), StringComparison.OrdinalIgnoreCase)
                && x.GetGenericArguments().Length != 0
                && x.GetParameters().Length == 4
            );
        var genericMethod = methodInfo.MakeGenericMethod(data.GetType());

        return (IMessageEnvelopeBase)genericMethod.Invoke(null, [data, correlationId, cautionId, headers])!;
    }
}
