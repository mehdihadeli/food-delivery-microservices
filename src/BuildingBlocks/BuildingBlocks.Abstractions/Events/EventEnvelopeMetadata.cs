namespace BuildingBlocks.Abstractions.Events;

public interface IEventEnvelopeMetadata
{
    Guid? MessageId { get; init; }
    string? MessageType { get; init; }
    string? Name { get; init; }
    string? CausationId { get; init; }
    Guid? CorrelationId { get; init; }
    DateTime Created { get; init; }
    long? CreatedUnixTime { get; init; }
    IDictionary<string, object?>? Headers { get; init; }
}
