using System.Globalization;
using System.Reflection;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace BuildingBlocks.Logging;

public static class RegistrationExtensions
{
    public static WebApplicationBuilder AddCustomSerilog(
        this WebApplicationBuilder builder,
        Action<LoggingOptionsBuilder>? optionBuilder = null,
        Action<LoggerConfiguration>? extraConfigure = null)
    {
        builder.Host.UseSerilog((context, serviceProvider, loggerConfiguration) =>
        {
            var loggerOptions = context.Configuration.GetOptions<LoggerOptions>();
            var appOptions = context.Configuration.GetOptions<AppOptions>();

            extraConfigure?.Invoke(loggerConfiguration);
            optionBuilder?.Invoke(new LoggingOptionsBuilder(loggerOptions));

            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                //.ReadFrom.Services(serviceProvider)
                .Enrich.WithProperty("Application", appOptions?.Name)
                // .Enrich.WithSpan()
                // .Enrich.WithBaggage()

                // .WriteTo.OpenTelemetry(new ConsoleSink())
                .Enrich.WithCorrelationIdHeader()
                .Enrich.FromLogContext()

                // https://github.com/serilog/serilog-enrichers-environment
                // .Enrich.WithEnvironmentName()
                // .Enrich.WithMachineName()

                // https://rehansaeed.com/logging-with-serilog-exceptions/
                .Enrich.WithExceptionDetails();
                //.ReadFrom.Configuration(context.Configuration);

            var level = Enum.TryParse<LogEventLevel>(loggerOptions?.Level, true, out var logLevel)
                ? logLevel
                : LogEventLevel.Information;

            // https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-reducing-log-verbosity/
            loggerConfiguration.MinimumLevel.Is(level)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel

                // Filter out ASP.NET Core infrastructure logs that are Information and below
                .Override("Microsoft.AspNetCore", LogEventLevel.Warning);

            if (context.HostingEnvironment.IsDevelopment())
            {
                // https://github.com/serilog/serilog-sinks-async
                loggerConfiguration.WriteTo.Async(writeTo => writeTo.Console(
                    level,
                    loggerOptions?.LogTemplate ??
                    "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception} {Properties:j}"
                ));
            }

            // https://github.com/serilog/serilog-sinks-async
            if (!string.IsNullOrEmpty(loggerOptions?.ElasticSearchUrl))
            {
                // https://github.com/serilog-contrib/serilog-sinks-elasticsearch
                loggerConfiguration.WriteTo.Async(
                    writeTo =>
                        writeTo.Elasticsearch(ConfigureElasticSink(
                            loggerOptions.ElasticSearchUrl,
                            context.HostingEnvironment.EnvironmentName)));
            }

            if (!string.IsNullOrEmpty(loggerOptions?.SeqUrl))
            {
                loggerConfiguration.WriteTo.Async(writeTo => writeTo.Seq(loggerOptions.SeqUrl));
            }

            if (!string.IsNullOrEmpty(loggerOptions?.LogPath))
            {
                loggerConfiguration.WriteTo.File(
                    loggerOptions.LogPath,
                    outputTemplate: loggerOptions.LogTemplate ??
                                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true);
            }
        });

        return builder;
    }

    private static ElasticsearchSinkOptions ConfigureElasticSink(string elasticUrl, string environment)
    {
        return new(new Uri(elasticUrl))
        {
            AutoRegisterTemplate = true,

            // we should add corresponding index in kibana also, for example : ecommerce-services-catalogs-api-*
            IndexFormat =
                $"{Assembly.GetEntryAssembly()?.GetName().Name?.ToLower(CultureInfo.InvariantCulture).Replace(".", "-", StringComparison.OrdinalIgnoreCase)}-{environment.ToLower(CultureInfo.InvariantCulture).Replace(".", "-", StringComparison.OrdinalIgnoreCase)}-{DateTime.UtcNow:yyyy-MM}"
        };
    }
}
