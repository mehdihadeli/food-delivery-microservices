using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.Extensions;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace BuildingBlocks.Caching.Redis;

public static class Extensions
{
    public static IServiceCollection AddCustomRedisCache(
        this IServiceCollection services,
        IConfiguration config,
        Action<RedisCacheOptions>? configureOptions = null)
    {
        var redisOptions = config.GetOptions<RedisCacheOptions>(nameof(RedisCacheOptions));

        Guard.Against.Null(redisOptions);
        Guard.Against.NullOrEmpty(
            redisOptions.ConnectionString,
            nameof(redisOptions),
            "redis connection string can't be null");

        if (configureOptions is { })
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.AddOptions<RedisCacheOptions>().Bind(config.GetSection(nameof(RedisCacheOptions)))
                .ValidateDataAnnotations();
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints = {redisOptions.ConnectionString},
                AbortOnConnectFail = false,
                DefaultDatabase = redisOptions.Db,
            }));

        services.AddSingleton<IRedisCache, RedisCache>();
        services.AddSingleton<ICacheManager, CacheManager>();
        services.AddSingleton<ICacheProvider, RedisCacheProvider>();

        return services;
    }
}
