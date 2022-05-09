namespace BuildingBlocks.Core.Messaging;

public static class MessageHeaders
{
    public const string MessageId = "message-id";
    public const string CorrelationId = "correlation-id";
    public const string CausationId = "causation-id";
    public const string TraceId = "trace-id";
    public const string SpanId = "span-id";
    public const string ParentSpanId = "parent-id";
    public const string Name = "name";
    public const string Type = "type";
    public const string Created = "created";
}
