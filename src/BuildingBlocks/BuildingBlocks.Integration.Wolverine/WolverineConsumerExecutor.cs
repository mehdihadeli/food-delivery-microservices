using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Web.Extensions;
using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Wolverine;

namespace BuildingBlocks.Integration.Wolverine;

public static class WolverineConsumerExecutor
{
    public static async Task ExecuteAsync<TMessage>(
        TMessage message,
        Envelope transportEnvelope,
        HeaderPropagationValues headerPropagationValues,
        Func<TMessage, Task> dispatch,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
        where TMessage : class
    {
        InitializeHeaderPropagation(headerPropagationValues);

        var correlationIdValue = Convert.ToString(transportEnvelope.CorrelationId);
        if (Guid.TryParse(correlationIdValue, out var correlationId))
        {
            headerPropagationValues.AddCorrelationId(correlationId);
        }

        var messageIdValue = Convert.ToString(transportEnvelope.Id);
        if (Guid.TryParse(messageIdValue, out var messageId))
        {
            headerPropagationValues.AddMessageId(messageId);
        }

        IMessageEnvelopeBase messageEnvelope;
        if (message is IMessageEnvelopeBase existingEnvelope)
        {
            messageEnvelope = existingEnvelope;
        }
        else
        {
            var metadata = new MessageEnvelopeMetadata(
                Guid.TryParse(messageIdValue, out var parsedMessageId) ? parsedMessageId : Guid.CreateVersion7(),
                Guid.TryParse(correlationIdValue, out var parsedCorrelationId)
                    ? parsedCorrelationId
                    : Guid.CreateVersion7(),
                transportEnvelope.MessageType,
                message.GetType().Name,
                Guid.TryParse(Convert.ToString(transportEnvelope.ParentId), out var parsedCausationId)
                    ? parsedCausationId
                    : null
            );

            foreach (var header in transportEnvelope.Headers)
            {
                metadata.Headers[header.Key] = header.Value;
            }

            messageEnvelope = MessageEnvelopeFactory.From(message, metadata);
        }

        await dispatch(message).ConfigureAwait(false);

        logger?.LogInformation("Message with ID {MessageId} processed and marked as delivered.", messageIdValue);
    }

    private static void InitializeHeaderPropagation(HeaderPropagationValues headerPropagationValues)
    {
        headerPropagationValues.Headers ??= new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
    }
}
