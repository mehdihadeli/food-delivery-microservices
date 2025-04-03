namespace BuildingBlocks.Core.Diagnostics;

public static class DiagnosticsConstant
{
    public static string? ApplicationInstrumentationName;

    public static class Components
    {
        public const string CommandHandler = "CommandHandler";
        public const string QueryHandler = "QueryHandler";
        public const string EventStore = "EventStore";
        public const string Producer = "Producer";
        public const string Consumer = "Consumer";
        public const string EventHandler = "EventHandler";
    }
}
