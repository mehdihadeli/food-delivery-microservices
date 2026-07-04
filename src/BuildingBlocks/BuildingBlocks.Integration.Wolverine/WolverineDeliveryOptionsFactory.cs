using System.Globalization;
using BuildingBlocks.Abstractions.Messages;
using Wolverine;
using MessageHeaders = BuildingBlocks.Core.Messages.MessageHeaders;

namespace BuildingBlocks.Integration.Wolverine;

internal static class WolverineDeliveryOptionsFactory
{
    internal static DeliveryOptions Build(IMessageEnvelopeBase messageEnvelope)
    {
        var deliveryOptions = new DeliveryOptions
        {
            CorrelationId = messageEnvelope.Metadata.CorrelationId.ToString(),
            CausationId = messageEnvelope.Metadata.CausationId?.ToString(),
        };

        deliveryOptions.WithHeader(MessageHeaders.MessageId, messageEnvelope.Metadata.MessageId.ToString());
        deliveryOptions.WithHeader(MessageHeaders.Type, messageEnvelope.Metadata.MessageType);
        deliveryOptions.WithHeader(MessageHeaders.Name, messageEnvelope.Metadata.Name);
        deliveryOptions.WithHeader(MessageHeaders.CausationId, messageEnvelope.Metadata.CausationId.ToString());
        deliveryOptions.WithHeader(MessageHeaders.CorrelationId, messageEnvelope.Metadata.CorrelationId.ToString());
        deliveryOptions.WithHeader(
            MessageHeaders.Created,
            messageEnvelope.Metadata.Created.ToString(CultureInfo.InvariantCulture)
        );

        foreach (var header in messageEnvelope.Metadata.Headers)
        {
            if (header.Value is not null)
            {
                deliveryOptions.WithHeader(header.Key, header.Value.ToString()!);
            }
        }

        return deliveryOptions;
    }
}
