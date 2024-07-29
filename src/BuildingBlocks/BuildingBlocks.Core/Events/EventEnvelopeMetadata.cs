using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Types.Extensions;

namespace BuildingBlocks.Core.Events;

public record EventEnvelopeMetadata(Guid MessageId, Guid CorrelationId, string MessageType, string Name)
    : IEventEnvelopeMetadata
{
    public IDictionary<string, object?> Headers { get; init; } = new Dictionary<string, object?>();
    public string? CausationId { get; init; }
    public DateTime Created { get; init; } = DateTime.Now;
    public long? CreatedUnixTime { get; init; } = DateTime.Now.ToUnixTimeSecond();
}
