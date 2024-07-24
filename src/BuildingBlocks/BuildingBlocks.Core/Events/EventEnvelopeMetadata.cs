using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Types.Extensions;

namespace BuildingBlocks.Core.Events;

public record EventEnvelopeMetadata(
    Guid? MessageId,
    Guid? CorrelationId,
    string? MessageType,
    string? Name = null,
    IDictionary<string, object?>? Headers = null
) : IEventEnvelopeMetadata
{
    public string? CausationId { get; init; }
    public DateTime Created { get; init; } = DateTime.Now;
    public long? CreatedUnixTime { get; init; } = DateTime.Now.ToUnixTimeSecond();
}
