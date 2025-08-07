using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.HostApplicationBuilderExtensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Persistence.EventStore.Extensions;
using BuildingBlocks.Persistence.EventStoreDB.Subscriptions;
using EventStore.Client;
using EventStore.Client.Extensions.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Persistence.EventStoreDB.Extensions;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddEventStoreDb(
        this IHostApplicationBuilder builder,
        string? connectionStringName = null
    )
    {
        var options = builder.Configuration.BindOptions<EventStoreDbOptions>();

        // add an option to the dependency injection
        builder.Services.AddConfigurationOptions<EventStoreDbOptions>();

        // ConnectionString is not injected by aspire
        if (
            string.IsNullOrWhiteSpace(connectionStringName)
            || string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(connectionStringName))
        )
        {
            var connectionString =
                options.GrpcConnectionString
                ?? throw new InvalidOperationException("`EventStoreDbOptions.GrpcConnectionString` can't be null.");

            builder.Services.TryAddSingleton(new EventStoreClient(EventStoreClientSettings.Create(connectionString)));

            builder.ConfigureInstrumentation(options, connectionString);
        }
        else
        {
            // - EnvironmentVariablesConfigurationProvider is injected by aspire and use to read configuration values from environment variables with `ConnectionStrings:event-store` key on configuration.
            // The configuration provider handles these conversions automatically, and `__ (double underscore)` becomes `:` for nested sections,
            // so environment configuration reads its data from the ` ConnectionStrings__event-store ` environment. all envs are available in `Environment.GetEnvironmentVariables()`.
            // - For setting none sensitive configuration, we can use Aspire named configuration `Aspire:EventStore:Client:DisableHealthChecks` which is of type ConfigurationProvider and should be set in appsetting.json

            // https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-eventstore?tabs=dotnet-cli#client-integration
            // ConnectionString injected by aspire
            builder.AddEventStoreClient(connectionStringName);
        }

        builder.Services.AddEventSourcing<EventStoreDbEventStore>();

        if (options.UseInternalCheckpointing)
        {
            builder.Services.TryAddTransient<
                ISubscriptionCheckpointRepository,
                EventStoreDbSubscriptionCheckPointRepository
            >();
        }

        return builder;
    }

    public static IServiceCollection AddEventStoreDbSubscriptionToAll(
        this IHostApplicationBuilder builder,
        bool checkpointToEventStoreDb = true
    )
    {
        if (checkpointToEventStoreDb)
        {
            builder.Services.TryAddTransient<
                ISubscriptionCheckpointRepository,
                EventStoreDbSubscriptionCheckPointRepository
            >();
        }

        return builder.Services.AddHostedService<EventStoreDbSubscriptionToAll>();
    }

    private static void ConfigureInstrumentation(
        this IHostApplicationBuilder builder,
        EventStoreDbOptions eventStoreDbOptions,
        string connectionString
    )
    {
        if (!eventStoreDbOptions.DisableTracing)
        {
            builder
                .Services.AddOpenTelemetry()
                // add eventstoredb official tracing extension
                .WithTracing(traceBuilder => traceBuilder.AddEventStoreClientInstrumentation());
        }

        if (!eventStoreDbOptions.DisableHealthChecks)
        {
            builder.TryAddHealthCheck(
                "EventStore.Client",
                healthCheckRegistration =>
                {
                    healthCheckRegistration.AddEventStore(
                        connectionString: connectionString,
                        name: "EventStore.Client",
                        failureStatus: default,
                        tags: ["live"]
                    );
                }
            );
        }
    }
}
