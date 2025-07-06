using BuildingBlocks.Core.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.HealthCheck;

// https://dev.to/dbolotov/observability-with-grafana-cloud-and-opentelemetry-in-net-microservices-448c
// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks
// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults
public static class DependencyInjectionExtensions
{
    private const string HealthChecks = nameof(HealthChecks);

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        var healthCheckOptions = builder.Configuration.BindOptions<HealthCheckOptions>();

        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
        builder.Services.AddRequestTimeouts(configure: timeouts =>
            timeouts.AddPolicy(HealthChecks, TimeSpan.FromSeconds(healthCheckOptions.RequestTimeoutSecond))
        );

        builder.Services.AddOutputCache(configureOptions: caching =>
            caching.AddPolicy(
                HealthChecks,
                build: policy => policy.Expire(TimeSpan.FromSeconds(healthCheckOptions.ExpireAfterSecond))
            )
        );

        builder
            .Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
            .AddDiskStorageHealthCheck(_ => { }, tags: ["live"])
            .AddPingHealthCheck(_ => { }, tags: ["live"])
            .AddPrivateMemoryHealthCheck(512 * 1024 * 1024, tags: ["live"])
            .AddDnsResolveHealthCheck(_ => { }, tags: ["live"])
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
            });

        // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/#healthcheckui
        builder.Services.AddHealthChecksUI().AddInMemoryStorage();

        return builder;
    }
}
