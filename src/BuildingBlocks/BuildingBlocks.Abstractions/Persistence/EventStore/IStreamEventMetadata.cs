namespace BuildingBlocks.Abstractions.Persistence.EventStore;

public interface IStreamEventMetadata
{
    string EventId { get; }
    long? LogPosition { get; }
    long StreamPosition { get; }
}
