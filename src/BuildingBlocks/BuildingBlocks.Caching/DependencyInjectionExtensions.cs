using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Caching.Serializers.MessagePack;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace BuildingBlocks.Caching;

#pragma warning disable EXTEXP0018

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddCustomCaching(this IHostApplicationBuilder builder)
    {
        builder.NotBeNull();

        builder.Services.AddConfigurationOptions<CacheOptions>(nameof(CacheOptions));
        var cacheOptions = builder.Configuration.BindOptions<CacheOptions>(nameof(CacheOptions));

        builder.Services.AddSingleton<ICacheService, CacheService>();

        // https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid
        // https://learn.microsoft.com/en-us/aspnet/core/performance/caching/overview
        // If the app has an IDistributedCache implementation, the HybridCache service uses it for secondary caching. This two-level caching strategy allows HybridCache to provide the speed of an in-memory cache and the durability of a distributed or persistent cache.
        if (cacheOptions.UseRedisDistributedCache)
        {
            builder.Services.AddSingleton<IRedisPubSubService, RedisPubSubService>();
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                ArgumentNullException.ThrowIfNull(cacheOptions.RedisCacheOptions);
                return ConnectionMultiplexer.Connect(
                    new ConfigurationOptions
                    {
                        EndPoints = { $"{cacheOptions.RedisCacheOptions.Host}:{cacheOptions.RedisCacheOptions.Port}" },
                        AllowAdmin = cacheOptions.RedisCacheOptions.AllowAdmin,
                    }
                );
            });
        }

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

        // add redis distributed lock
        builder.Services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            ArgumentNullException.ThrowIfNull(cacheOptions.RedisCacheOptions);
            var redisConnection = ConnectionMultiplexer.Connect(
                new ConfigurationOptions
                {
                    EndPoints = { $"{cacheOptions.RedisCacheOptions.Host}:{cacheOptions.RedisCacheOptions.Port}" },
                    AllowAdmin = cacheOptions.RedisCacheOptions.AllowAdmin,
                }
            );

            return new RedisDistributedSynchronizationProvider(redisConnection.GetDatabase());
        });

        return builder;
    }
}
