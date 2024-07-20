using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using EasyCaching.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace BuildingBlocks.Caching;

public static class Extensions
{
    public static WebApplicationBuilder AddCustomEasyCaching(
        this WebApplicationBuilder builder,
        Action<CacheOptions>? configurator = null
    )
    {
        // https://www.twilio.com/blog/provide-default-configuration-to-dotnet-applications
        var cacheOptions = builder.Configuration.BindOptions<CacheOptions>();
        configurator?.Invoke(cacheOptions);

        // add option to the dependency injection
        builder.Services.AddValidationOptions<CacheOptions>(opt => configurator?.Invoke(opt));

        builder.Services.AddEasyCaching(option =>
        {
            if (cacheOptions.RedisCacheOptions is not null)
            {
                option.UseRedis(
                    config =>
                    {
                        config.DBConfig = new RedisDBOptions
                        {
                            Configuration = cacheOptions.RedisCacheOptions.ConnectionString
                        };
                        config.SerializerName = cacheOptions.SerializationType;
                    },
                    nameof(CacheProviderType.Redis)
                );
            }

            option.UseInMemory(
                config =>
                {
                    config.SerializerName = cacheOptions.SerializationType;
                },
                nameof(CacheProviderType.InMemory)
            );

            switch (cacheOptions.SerializationType)
            {
                case nameof(CacheSerializationType.Json):
                    option.WithJson(
                        jsonSerializerSettingsConfigure: x =>
                        {
                            x.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
                        },
                        nameof(CacheSerializationType.Json)
                    );
                    break;
                case nameof(CacheSerializationType.MessagePack):
                    option.WithMessagePack(nameof(CacheSerializationType.MessagePack));
                    break;
            }
        });

        return builder;
    }

    public static WebApplicationBuilder AddCustomRedis(
        this WebApplicationBuilder builder,
        Action<RedisOptions>? configurator = null
    )
    {
        // https://www.twilio.com/blog/provide-default-configuration-to-dotnet-applications
        var redisOptions = builder.Configuration.BindOptions<RedisOptions>();
        configurator?.Invoke(redisOptions);

        // add option to the dependency injection
        builder.Services.AddValidationOptions<RedisOptions>(opt => configurator?.Invoke(opt));

        builder.Services.TryAddSingleton<IConnectionMultiplexer>(
            sp =>
                ConnectionMultiplexer.Connect(
                    new ConfigurationOptions
                    {
                        EndPoints = { $"{redisOptions.Host}:{redisOptions.Port}" },
                        AllowAdmin = true
                    }
                )
        );

        return builder;
    }
}
