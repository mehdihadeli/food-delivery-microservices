using OpenTelemetry.Context.Propagation;

namespace BuildingBlocks.Abstractions.Messages;

public record StreamEventMetadata(
    string EventId,
    ulong StreamPosition,
    ulong? LogPosition,
    PropagationContext? PropagationContext = null
);
