namespace BuildingBlocks.Core.Persistence.EventStore;

public class StreamEventData
{
    /// <summary>
    /// Gets or sets unique event identifier.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Gets or sets Stream identifier.
    /// </summary>
    public string StreamId { get; set; }

    /// <summary>
    /// Gets or sets event name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets event data as byte array.
    /// </summary>
    public byte[] Data { get; set; } = null!;

    /// <summary>
    /// Gets or sets event metadata like correlation id as byte array.
    /// </summary>
    public byte[]? Metadata { get; set; }

    /// <summary>
    /// Gets or sets name of the event, e.g. "invoice_issued".
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content type event like application/json.
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// Gets or sets representing a time at which the event happened.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the position of this event in the $all stream.
    /// </summary>
    public int GlobalEventPosition { get; set; }

    /// <summary>
    ///  Gets or sets the event number of this event in the stream (StreamPosition).
    /// (also named version, order of occurrence, etc.): the number used to decide the order of the event's occurrence for the specific object (stream).
    /// </summary>
    public int EventNumber { get; set; }
}
