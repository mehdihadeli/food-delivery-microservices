using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Caching.Serializers.MessagePack;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.HostApplicationBuilderExtensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Instrumentation.StackExchangeRedis;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace BuildingBlocks.Caching.Extensions;

#pragma warning disable EXTEXP0018

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddCustomCaching(
        this IHostApplicationBuilder builder,
        string? redisConnectionStringName = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddConfigurationOptions<CacheOptions>(nameof(CacheOptions));
        var cacheOptions = builder.Configuration.BindOptions<CacheOptions>(nameof(CacheOptions));

        AddRedis(builder, redisConnectionStringName, cacheOptions.RedisDistributedCacheOptions);

        builder.Services.AddSingleton<ICacheService, CacheService>();

        var hybridCacheBuilder = builder.Services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = cacheOptions.MaximumPayloadBytes;
            options.MaximumKeyLength = cacheOptions.MaximumKeyLength;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(cacheOptions.ExpirationTimeInMinute),
                LocalCacheExpiration = TimeSpan.FromMinutes(cacheOptions.ExpirationTimeInMinute),
            };
        });

        switch (cacheOptions.SerializationType)
        {
            case CacheSerializationType.MessagePack:
                {
                    hybridCacheBuilder.AddSerializerFactory<MessagePackHybridCacheSerializerFactory>();
                }

                break;
        }

        return builder;
    }

    private static void AddRedis(
        IHostApplicationBuilder builder,
        string? redisConnectionStringName,
        RedisDistributedCacheOptions redisDistributedCacheOptions
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        string redisConnectionString;

        // ConnectionString is not injected by aspire
        if (
            redisConnectionStringName is null
            || string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(redisConnectionStringName))
        )
        {
            redisConnectionString =
                redisDistributedCacheOptions.ConnectionString
                ?? throw new InvalidOperationException(
                    "`RedisDistributedCacheOptions.ConnectionString` can't be null."
                );

            // https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid
            // https://learn.microsoft.com/en-us/aspnet/core/performance/caching/overview
            // If the app has an IDistributedCache implementation, the HybridCache service uses it for secondary caching. This two-level caching strategy allows HybridCache to provide the speed of an in-memory cache and the durability of a distributed or persistent cache.
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp => CreateConnection(sp, redisConnectionString));

            ConfigureRedisInstrumentation(builder, redisDistributedCacheOptions);
        }
        else
        {
            // - EnvironmentVariablesConfigurationProvider is injected by aspire and use to read configuration values from environment variables with `ConnectionStrings:redis` key on configuration.
            // The configuration provider handles these conversions automatically, and `__ (double underscore)` becomes `:` for nested sections, so environment configuration reads its data from the ` ConnectionStrings__redis ` environment. all envs are available in `Environment.GetEnvironmentVariables()`.
            // - For setting none sensitive configuration, we can use Aspire named configuration `Aspire:StackExchange:Redis:DisableHealthChecks` which is of type ConfigurationProvider and should be set in appsetting.json
            redisConnectionString =
                builder.Configuration.GetConnectionString(redisConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Redis connection string '{redisConnectionStringName}' not found."
                );

            // ConnectionString is injected by aspire
            // https://learn.microsoft.com/en-us/dotnet/aspire/caching/stackexchange-redis-integration?tabs=dotnet-cli&pivots=redis#client-integration
            builder.AddRedisClient(redisConnectionStringName);
        }

        builder.Services.AddSingleton<IRedisPubSubService, RedisPubSubService>();
        // add redis distributed lock
        builder.Services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var redisConnection = CreateConnection(sp, redisConnectionString);

            return new RedisDistributedSynchronizationProvider(redisConnection.GetDatabase());
        });
    }

    private static void ConfigureRedisInstrumentation(
        IHostApplicationBuilder builder,
        RedisDistributedCacheOptions redisDistributedCacheOptions
    )
    {
        if (!redisDistributedCacheOptions.DisableTracing)
        {
            builder
                .Services.AddOpenTelemetry()
                .WithTracing(t =>
                {
                    t.AddSource("OpenTelemetry.Instrumentation.StackExchangeRedis");
                    // This ensures the core Redis instrumentation services from OpenTelemetry.Instrumentation.StackExchangeRedis are added
                    t.ConfigureRedisInstrumentation(_ => { });
                    // This ensures that any logic performed by the AddInstrumentation method is executed (this is usually called by AddRedisInstrumentation())
                    t.AddInstrumentation(sp => sp.GetRequiredService<StackExchangeRedisInstrumentation>());
                });
        }

        if (!redisDistributedCacheOptions.DisableHealthChecks)
        {
            builder.TryAddHealthCheck(
                name: "StackExchange.Redis",
                healthCheckRegistration =>
                    healthCheckRegistration.AddRedis(
                        connectionMultiplexerFactory: sp => sp.GetRequiredService<IConnectionMultiplexer>(),
                        name: "StackExchange.Redis",
                        tags: ["live"]
                    )
            );
        }
    }

    private static ConnectionMultiplexer CreateConnection(IServiceProvider serviceProvider, string connectionString)
    {
        var configurationOptions = ConfigurationOptions.Parse(connectionString);

        var connection = ConnectionMultiplexer.Connect(configurationOptions);

        // Add the connection to instrumentation
        var instrumentation = serviceProvider.GetService<StackExchangeRedisInstrumentation>();
        instrumentation?.AddConnection(connection);

        return connection;
    }
}
