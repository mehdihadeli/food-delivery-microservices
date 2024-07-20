using System.Reflection;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Persistence.Marten.Subscriptions;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Weasel.Core;

namespace BuildingBlocks.Persistence.Marten.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMartenDB(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<StoreOptions>? configureOptions = null,
        Action<MartenOptions>? configurator = null,
        params Assembly[] scanAssemblies
    )
    {
        var assembliesToScan = scanAssemblies.Any() ? scanAssemblies : new[] { Assembly.GetCallingAssembly(), };

        var martenOptions = configuration.BindOptions<MartenOptions>();
        configurator?.Invoke(martenOptions);

        // add option to the dependency injection
        services.AddValidationOptions<MartenOptions>(opt => configurator?.Invoke(opt));

        services.AddEventSourcing<MartenEventStore>(ServiceLifetime.Scoped, assembliesToScan);

        services
            .AddMarten(sp =>
            {
                var storeOptions = new StoreOptions();
                storeOptions.Connection(martenOptions.ConnectionString);
                storeOptions.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

                var schemaName = Environment.GetEnvironmentVariable("SchemaName");
                storeOptions.Events.DatabaseSchemaName = schemaName ?? martenOptions.WriteModelSchema;
                storeOptions.DatabaseSchemaName = schemaName ?? martenOptions.ReadModelSchema;

                storeOptions.UseDefaultSerialization(
                    EnumStorage.AsString,
                    nonPublicMembersStorage: NonPublicMembersStorage.All
                );

                storeOptions.Projections.Add(
                    new MartenSubscription(
                        new[] { new MartenEventPublisher(sp.GetRequiredService<IMediator>()) },
                        sp.GetRequiredService<ILogger<MartenSubscription>>()
                    ),
                    ProjectionLifecycle.Async,
                    "MartenSubscription"
                );

                if (martenOptions.UseMetadata)
                {
                    storeOptions.Events.MetadataConfig.CausationIdEnabled = true;
                    storeOptions.Events.MetadataConfig.CorrelationIdEnabled = true;
                    storeOptions.Events.MetadataConfig.HeadersEnabled = true;
                }

                configureOptions?.Invoke(storeOptions);

                return storeOptions;
            })
            .UseLightweightSessions()
            .ApplyAllDatabaseChangesOnStartup()
            //.OptimizeArtifactWorkflow()
            .AddAsyncDaemon(DaemonMode.Solo);

        return services;
    }
}
