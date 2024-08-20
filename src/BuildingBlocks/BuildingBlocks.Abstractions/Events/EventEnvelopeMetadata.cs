namespace BuildingBlocks.Abstractions.Events;

public record EventEnvelopeMetadata(
    Guid MessageId,
    Guid CorrelationId,
    string MessageType,
    string Name,
    // Causation ID identifies messages that cause other messages to be published. In simple terms, it's used to see what causes what. The first message in a message conversation typically doesn't have a causation ID. Downstream messages get their causation IDs by copying message IDs from messages, causing downstream messages to be published
    Guid? CausationId
)
{
    public IDictionary<string, object?> Headers { get; init; } = new Dictionary<string, object?>();
    public DateTime Created { get; init; } = DateTime.Now;
    public long? CreatedUnixTime { get; init; } = DateTimeHelper.ToUnixTimeSecond(DateTime.Now);

    internal static class DateTimeHelper
    {
        private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimeSecond(DateTime datetime)
        {
            var unixTime = (datetime.ToUniversalTime() - _epoch).TotalSeconds;
            return (long)unixTime;
        }
    }
}
