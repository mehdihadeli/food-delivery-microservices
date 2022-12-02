using System.Globalization;
using System.Reflection;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
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

            Enum.TryParse<LogEventLevel>(loggerOptions.Level, true, out var logLevel);

            loggerConfiguration
                .Enrich.WithProperty("Application", appOptions.Name)

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

            // https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-reducing-log-verbosity/
            loggerConfiguration.MinimumLevel.Is(logLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)

                // Filter out ASP.NET Core infrastructure logs that are Information and below
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

            if (context.HostingEnvironment.IsDevelopment())
            {
                // https://github.com/serilog/serilog-sinks-async
                loggerConfiguration.WriteTo.Async(writeTo => writeTo.Console(
                    logLevel,
                    loggerOptions.LogTemplate
                ));
            }

            // https://github.com/serilog/serilog-sinks-async
            if (!string.IsNullOrEmpty(loggerOptions.ElasticSearchUrl))
            {
                // https://github.com/serilog-contrib/serilog-sinks-elasticsearch
                loggerConfiguration.WriteTo.Async(
                    writeTo =>
                        writeTo.Elasticsearch(ConfigureElasticSink(
                            loggerOptions.ElasticSearchUrl,
                            context.HostingEnvironment.EnvironmentName)));
            }

            if (!string.IsNullOrEmpty(loggerOptions.SeqUrl))
            {
                loggerConfiguration.WriteTo.Async(writeTo => writeTo.Seq(loggerOptions.SeqUrl));
            }

            if (!string.IsNullOrEmpty(loggerOptions?.LogPath))
            {
                loggerConfiguration.WriteTo.File(
                    loggerOptions.LogPath,
                    outputTemplate: loggerOptions.LogTemplate,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true);
            }

            // https://github.com/serilog/serilog-settings-configuration
            loggerConfiguration.ReadFrom.Configuration(context.Configuration, sectionName: nameof(LoggerOptions));
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
