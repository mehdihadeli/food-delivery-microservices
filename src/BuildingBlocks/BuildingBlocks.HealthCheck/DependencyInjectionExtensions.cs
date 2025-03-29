using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.HealthCheck;

// https://dev.to/dbolotov/observability-with-grafana-cloud-and-opentelemetry-in-net-microservices-448c

public static class DependencyInjectionExtensions
{
    private static readonly string[] _defaultTags = ["live", "ready"];

    public static WebApplicationBuilder AddCustomHealthCheck(
        this WebApplicationBuilder builder,
        Action<IHealthChecksBuilder>? healthChecksBuilder = null
    )
    {
        if (builder.Environment.IsDevelopment())
        {
            // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
            builder.Services.AddRequestTimeouts(configure: static timeouts =>
                timeouts.AddPolicy("HealthChecks", TimeSpan.FromSeconds(5))
            );

            builder.Services.AddOutputCache(configureOptions: static caching =>
                caching.AddPolicy("HealthChecks", build: static policy => policy.Expire(TimeSpan.FromSeconds(10)))
            );

            var healCheckBuilder = builder
                .Services.AddHealthChecks()
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
                .AddDiskStorageHealthCheck(_ => { }, tags: _defaultTags)
                .AddPingHealthCheck(_ => { }, tags: _defaultTags)
                .AddPrivateMemoryHealthCheck(512 * 1024 * 1024, tags: _defaultTags)
                .AddDnsResolveHealthCheck(_ => { }, tags: _defaultTags)
                .AddResourceUtilizationHealthCheck(o =>
                {
                    o.CpuThresholds = new ResourceUsageThresholds
                    {
                        DegradedUtilizationPercentage = 80,
                        UnhealthyUtilizationPercentage = 90,
                    };

                    o.MemoryThresholds = new ResourceUsageThresholds
                    {
                        DegradedUtilizationPercentage = 80,
                        UnhealthyUtilizationPercentage = 90,
                    };

                    o.SamplingWindow = TimeSpan.FromSeconds(5);
                });

            healthChecksBuilder?.Invoke(healCheckBuilder);

            builder
                .Services.AddHealthChecksUI(setup =>
                {
                    setup.SetEvaluationTimeInSeconds(60); // time in seconds between check
                    setup.AddHealthCheckEndpoint("All Checks", "/healthz");
                    setup.AddHealthCheckEndpoint("Infra", "/health/infra");
                    setup.AddHealthCheckEndpoint("Bus", "/health/bus");
                    setup.AddHealthCheckEndpoint("Database", "/health/database");

                    setup.AddHealthCheckEndpoint("Downstream Services", "/health/downstream-services");
                })
                .AddInMemoryStorage();
        }

        return builder;
    }
}
