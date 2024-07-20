using OpenTelemetry.Context.Propagation;

namespace BuildingBlocks.Abstractions.Persistence.EventStore;

public interface IStreamEventMetadata
{
    string EventId { get; }
    ulong? LogPosition { get; }
    ulong StreamPosition { get; }
    PropagationContext? PropagationContext { get; }
}
