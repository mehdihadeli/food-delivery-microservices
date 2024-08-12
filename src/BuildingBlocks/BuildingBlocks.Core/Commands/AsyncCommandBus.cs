using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Events;

namespace BuildingBlocks.Core.Commands;

public class AsyncCommandBus(
    IMessagePersistenceService messagePersistenceService,
    IMessageMetadataAccessor messageMetadataAccessor
) : IAsyncCommandBus
{
    public Task SendExternalAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : IAsyncCommand
    {
        var correlationId = messageMetadataAccessor.GetCorrelationId();
        var cautionId = messageMetadataAccessor.GetCautionId();
        var eventEnvelope = EventEnvelope.From(command, correlationId, cautionId);

        return messagePersistenceService.AddPublishMessageAsync(eventEnvelope, cancellationToken);
    }
}
