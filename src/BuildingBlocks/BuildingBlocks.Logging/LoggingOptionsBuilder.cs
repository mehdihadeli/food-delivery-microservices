using Serilog.Events;

namespace BuildingBlocks.Logging;

public class LoggingOptionsBuilder
{
    private readonly LoggerOptions _options;

    public LoggingOptionsBuilder(LoggerOptions? options)
    {
        _options = options ?? new LoggerOptions();
    }

    public LoggingOptionsBuilder SetSeqUrl(string url)
    {
        _options.SeqUrl = url;
        return this;
    }

    public LoggingOptionsBuilder SetElasticUrl(string url)
    {
        _options.ElasticSearchUrl = url;
        return this;
    }

    public LoggingOptionsBuilder SetLevel(LogEventLevel level)
    {
        _options.Level = level.ToString();
        return this;
    }

    public LoggingOptionsBuilder SetLogTemplate(string template)
    {
        _options.LogTemplate = template;
        return this;
    }

    public LoggingOptionsBuilder SetDevelopmentLogPath(string developmentLogPath)
    {
        _options.DevelopmentLogPath = developmentLogPath;
        return this;
    }

    public LoggingOptionsBuilder SetProductionLogPath(string productionLogPath)
    {
        _options.ProductionLogPath = productionLogPath;
        return this;
    }

    public LoggerOptions Build()
    {
        return _options;
    }
}
