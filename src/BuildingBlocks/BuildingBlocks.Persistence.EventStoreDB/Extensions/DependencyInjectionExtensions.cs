using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Persistence.EventStore;
using BuildingBlocks.Core.Persistence.EventStore.Extenions;
using BuildingBlocks.Persistence.EventStoreDB.Subscriptions;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Persistence.EventStoreDB.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddEventStoreDb(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<EventStoreDbOptions>? configurator = null
    )
    {
        var options = configuration.BindOptions<EventStoreDbOptions>();
        configurator?.Invoke(options);

        // add option to the dependency injection
        services.AddValidationOptions<EventStoreDbOptions>(opt => configurator?.Invoke(opt));

        services.TryAddSingleton(new EventStoreClient(EventStoreClientSettings.Create(options.GrpcConnectionString)));

        services.AddEventSourcing<EventStoreDbEventStore>();

        if (options.UseInternalCheckpointing)
        {
            services.TryAddTransient<ISubscriptionCheckpointRepository, EventStoreDbSubscriptionCheckPointRepository>();
        }

        return services;
    }

    public static IServiceCollection AddEventStoreDbSubscriptionToAll(
        this IServiceCollection services,
        bool checkpointToEventStoreDb = true
    )
    {
        if (checkpointToEventStoreDb)
        {
            services.TryAddTransient<ISubscriptionCheckpointRepository, EventStoreDbSubscriptionCheckPointRepository>();
        }

        return services.AddHostedService<EventStoreDbSubscriptionToAll>();
    }
}
