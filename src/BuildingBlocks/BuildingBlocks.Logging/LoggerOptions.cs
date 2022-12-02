using Serilog.Events;

namespace BuildingBlocks.Logging;

public class LoggerOptions
{
    public string Level { get; set; } = nameof(LogEventLevel.Information);
    public string? SeqUrl { get; set; }
    public string? ElasticSearchUrl { get; set; }

    public string LogTemplate { get; set; } =
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception} {Properties:j}";
    public string? LogPath { get; set; }
}
