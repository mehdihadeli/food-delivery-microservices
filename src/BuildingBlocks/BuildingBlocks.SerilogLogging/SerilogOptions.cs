namespace BuildingBlocks.SerilogLogging;

public sealed class SerilogOptions
{
    public bool Enabled { get; set; }
    public string? SeqUrl { get; set; }
    public bool UseConsole { get; set; } = true;
    public bool ExportLogsToOpenTelemetry { get; set; } = true;
    public string? ElasticSearchUrl { get; set; }
    public string? GrafanaLokiUrl { get; set; }
    public string LogTemplate { get; set; } =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}";
    public string? LogPath { get; set; }
}
