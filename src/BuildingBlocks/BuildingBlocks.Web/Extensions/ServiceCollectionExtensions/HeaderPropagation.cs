using BuildingBlocks.Web.HeaderPropagation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Web.Extensions;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Applying headers propagation to all clients.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddHeaderPropagation(
        this IServiceCollection services,
        Action<CustomHeaderPropagationOptions> configureOptions
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddHeaderPropagationCore(configureOptions);

        // for applying message handler to all http clients
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHttpMessageHandlerBuilderFilter,
                HeaderPropagationMessageHandlerBuilderFilter
            >()
        );

        return services;
    }

    /// <summary>
    /// Applying headers propagation to specific client.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddHeaderPropagation(
        this IHttpClientBuilder builder,
        Action<CustomHeaderPropagationOptions> configure
    )
    {
        builder.Services.AddHeaderPropagationCore(configure);
        builder.AddHttpMessageHandler(
            (sp) =>
            {
                var options = sp.GetRequiredService<IOptions<CustomHeaderPropagationOptions>>();
                var headers = sp.GetRequiredService<CustomHeaderPropagationStore>();

                return new HeaderPropagationMessageHandler(options.Value, headers);
            }
        );

        return builder;
    }

    private static IServiceCollection AddHeaderPropagationCore(
        this IServiceCollection services,
        Action<CustomHeaderPropagationOptions> configureOptions
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.TryAddSingleton<CustomHeaderPropagationStore>();

        services.Configure(configureOptions);

        return services;
    }
}
