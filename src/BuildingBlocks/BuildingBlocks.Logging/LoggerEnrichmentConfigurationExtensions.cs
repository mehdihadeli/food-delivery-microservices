using System.Diagnostics;
using Serilog;
using Serilog.Configuration;

namespace BuildingBlocks.Logging;

public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Enrich logger output with Baggage information from the current <see cref="Activity"/>.
    /// </summary>
    /// <param name="loggerEnrichmentConfiguration">The enrichment configuration.</param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration WithBaggage(
        this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration)
    {
        ArgumentNullException.ThrowIfNull(loggerEnrichmentConfiguration);
        return loggerEnrichmentConfiguration.With(new BaggageEnricher());
    }
}