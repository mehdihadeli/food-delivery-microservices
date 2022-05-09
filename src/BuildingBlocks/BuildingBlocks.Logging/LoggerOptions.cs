namespace BuildingBlocks.Logging;

public class LoggerOptions
{
    public string? Level { get; set; }
    public string? SeqUrl { get; set; }
    public string? ElasticSearchUrl { get; set; }
    public string? LogTemplate { get; set; }
    public string? DevelopmentLogPath { get; set; }
    public string? ProductionLogPath { get; set; }
}