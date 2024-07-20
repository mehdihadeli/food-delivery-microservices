using BuildingBlocks.Abstractions.Persistence.EventStore;
using OpenTelemetry.Context.Propagation;

namespace BuildingBlocks.Core.Persistence.EventStore;

public record StreamEventMetadata(
    string EventId,
    ulong StreamPosition,
    ulong? LogPosition,
    PropagationContext? PropagationContext
) : IStreamEventMetadata;
