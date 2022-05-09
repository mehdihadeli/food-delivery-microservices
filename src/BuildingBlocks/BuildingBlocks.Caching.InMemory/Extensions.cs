using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Caching.InMemory;

public static class Extensions
{
    public static IServiceCollection AddCustomInMemoryCache(
        this IServiceCollection services,
        IConfiguration config,
        Action<InMemoryCacheOptions>? configureOptions = null)
    {
        Guard.Against.Null(services, nameof(services));

        var options = config.GetOptions<InMemoryCacheOptions>(nameof(InMemoryCacheOptions));

        if (configureOptions is { })
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.AddOptions<InMemoryCacheOptions>().Bind(config.GetSection(nameof(InMemoryCacheOptions)))
                .ValidateDataAnnotations();
        }

        services.AddMemoryCache();
        services.AddTransient<ICacheManager, CacheManager>();
        services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();

        return services;
    }
}
