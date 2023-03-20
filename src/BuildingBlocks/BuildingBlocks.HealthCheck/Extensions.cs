using System.Text;
using System.Text.Json;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web.Extenions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

namespace BuildingBlocks.HealthCheck;

// https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
// https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
// https://nikiforovall.github.io/dotnet/aspnetcore/coding-stories/2021/07/25/add-health-checks-to-aspnetcore.html
// https://code-maze.com/health-checks-aspnetcore/
// https://github.com/prometheus-net/prometheus-net
public static class Extensions
{
    public static WebApplicationBuilder AddCustomHealthCheck(
        this WebApplicationBuilder builder,
        Action<IHealthChecksBuilder>? healthChecksBuilder = null,
        string sectionName = nameof(HealthOptions)
    )
    {
        var healthOptions = builder.Configuration.BindOptions<HealthOptions>(sectionName);
        if (!healthOptions.Enabled)
        {
            return builder;
        }

        var healCheckBuilder = builder.Services
            .AddHealthChecks()
            .AddDiskStorageHealthCheck(_ => { }, tags: new[] { "live", "ready" })
            .AddPingHealthCheck(_ => { }, tags: new[] { "live", "ready" })
            .AddPrivateMemoryHealthCheck(512 * 1024 * 1024, tags: new[] { "live", "ready" })
            .AddDnsResolveHealthCheck(_ => { }, tags: new[] { "live", "ready" })
            .ForwardToPrometheus();

        healthChecksBuilder?.Invoke(healCheckBuilder);

        builder.Services
            .AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(60); // time in seconds between check
                setup.AddHealthCheckEndpoint("All Checks", "/healthz");
                setup.AddHealthCheckEndpoint("Infra", "/health/infra");
                setup.AddHealthCheckEndpoint("Bus", "/health/bus");
                setup.AddHealthCheckEndpoint("Database", "/health/database");
                setup.AddHealthCheckEndpoint("Downstream Services", "/health/downstream-services");
            })
            .AddInMemoryStorage();

        return builder;
    }

    public static WebApplication UseCustomHealthCheck(this WebApplication app)
    {
        var healthOptions = app.Configuration.BindOptions<HealthOptions>();
        if (!healthOptions.Enabled)
        {
            return app;
        }

        app.UseHttpMetrics();
        app.UseGrpcMetrics();

        app.UseHealthChecks(
                "/healthz",
                new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
                    },
                }
            )
            .UseHealthChecks(
                "/health/infra",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("infra"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }
            )
            .UseHealthChecks(
                "/health/bus",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("bus"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }
            )
            .UseHealthChecks(
                "/health/database",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("database"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }
            )
            .UseHealthChecks(
                "/health/downstream-services",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("downstream-services"),
                    AllowCachingResponses = false,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                }
            )
            .UseHealthChecks(
                "health/ready",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                }
            )
            .UseHealthChecks(
                "health/live",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("live"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                }
            )
            .UseHealthChecksPrometheusExporter(
                "/health/prometheus",
                options =>
                {
                    options.ResultStatusCodes[HealthStatus.Unhealthy] = 200;
                }
            )
            .UseHealthChecksUI(setup =>
            {
                setup.ApiPath = "/healthcheck";
                setup.UIPath = "/healthcheck-ui";
            });

        return app;
    }

    private static Task WriteResponseAsync(HttpContext context, HealthReport result)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonWriterOptions { Indented = true };

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, options))
        {
            writer.WriteStartObject();
            writer.WriteString("status", result.Status.ToString());
            writer.WriteStartObject("results");
            foreach (var entry in result.Entries)
            {
                writer.WriteStartObject(entry.Key);
                writer.WriteString("status", entry.Value.Status.ToString());
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());

        return context.Response.WriteAsync(json, default);
    }
}
