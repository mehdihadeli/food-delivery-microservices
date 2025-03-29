using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Integration.MassTransit;

public class MessageHandlerConsumerFilter<T>(
    IMessagePersistenceService messagePersistenceService,
    IInternalEventBus internalEventBus,
    ILogger<MessageHandlerConsumerFilter<T>> logger
) : IFilter<ConsumeContext<T>>
    where T : class, IMessageEnvelopeBase
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var messageId = context.MessageId;

        // message broker deliver message just to one of consumer based on round-robin algorithm.
        await messagePersistenceService
            .AddReceivedMessageAsync<T>(
                context.Message,
                async (messageEnvelop) =>
                {
                    // Call the next filter in the pipeline (which will eventually call the consumer's Consume method)
                    await next.Send(context).ConfigureAwait(false);

                    // TODO: instead of `next.Send` and using consumers we call internal event bus publish in last pipeline for all auto registered consumers for all message handlers
                    // await internalEventBus.Publish(messageEnvelop, default);
                },
                context.CancellationToken
            )
            .ConfigureAwait(false);

        logger.LogInformation("Message with ID {MessageId} processed and marked as delivered.", messageId);
    }

    public void Probe(ProbeContext context) { }
}
