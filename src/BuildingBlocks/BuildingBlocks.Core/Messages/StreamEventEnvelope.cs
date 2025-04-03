using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Core.Messages;

public record StreamEventEnvelope<T>(T Data, StreamEventMetadata? Metadata) : IStreamEventEnvelope<T>
    where T : class, IDomainEvent
{
    object IStreamEventEnvelopeBase.Data => Data;
}
