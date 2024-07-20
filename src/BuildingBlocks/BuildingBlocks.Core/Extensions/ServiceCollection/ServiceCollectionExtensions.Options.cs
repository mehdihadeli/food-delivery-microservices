using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.Extensions.ServiceCollection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurationOptions<T>(
        this IServiceCollection services,
        Action<T> configuration
    )
        where T : class
    {
        var key = typeof(T).Name;

        return services.AddConfigurationOptions(key, configuration);
    }

    public static IServiceCollection AddConfigurationOptions<T>(
        this IServiceCollection services,
        string key,
        Action<T> configuration
    )
        where T : class
    {
        services.AddOptions<T>().BindConfiguration(key).Configure(configuration);

        services.TryAddSingleton(x => x.GetRequiredService<IOptions<T>>().Value);

        return services;
    }

    public static IServiceCollection AddValidationOptions<T>(
        this IServiceCollection services,
        Action<T>? configurator = null
    )
        where T : class
    {
        var key = typeof(T).Name;

        return AddValidatedOptions<T>(services, key, RequiredConfigurationValidator.Validate, configurator);
    }

    public static IServiceCollection AddValidationOptions<T>(
        this IServiceCollection services,
        string key,
        Action<T>? configurator = null
    )
        where T : class
    {
        return AddValidatedOptions<T>(services, key, RequiredConfigurationValidator.Validate, configurator);
    }

    public static IServiceCollection AddConfigurationOptions<T>(this IServiceCollection services)
        where T : class
    {
        return services.AddConfigurationOptions<T>(typeof(T).Name);
    }

    public static IServiceCollection AddConfigurationOptions<T>(this IServiceCollection services, string key)
        where T : class
    {
        services.AddOptions<T>().BindConfiguration(key);

        services.TryAddSingleton(x => x.GetRequiredService<IOptions<T>>().Value);

        return services;
    }

    public static IServiceCollection AddValidatedOptions<T>(this IServiceCollection services)
        where T : class
    {
        return AddValidatedOptions<T>(services, typeof(T).Name, RequiredConfigurationValidator.Validate);
    }

    public static IServiceCollection AddValidatedOptions<T>(this IServiceCollection services, string key)
        where T : class
    {
        return AddValidatedOptions<T>(services, key, RequiredConfigurationValidator.Validate);
    }

    public static IServiceCollection AddValidatedOptions<T>(
        this IServiceCollection services,
        string key,
        Func<T, bool> validator,
        Action<T>? configurator = null
    )
        where T : class
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options
        // https://thecodeblogger.com/2021/04/21/options-pattern-in-net-ioptions-ioptionssnapshot-ioptionsmonitor/
        // https://code-maze.com/aspnet-configuration-options/
        // https://code-maze.com/aspnet-configuration-options-validation/
        // https://dotnetdocs.ir/Post/42/difference-between-ioptions-ioptionssnapshot-and-ioptionsmonitor
        // https://andrewlock.net/adding-validation-to-strongly-typed-configuration-objects-in-dotnet-6/
        var configuration = services.AddOptions<T>().BindConfiguration(key);

        if (configurator is not null)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#configure-options-with-a-delegate
            configuration.Configure(configurator);
        }

        configuration.Validate(validator);

        // IOptions itself registered as singleton
        services.TryAddSingleton(x => x.GetRequiredService<IOptions<T>>().Value);

        return services;
    }
}

public static class RequiredConfigurationValidator
{
    public static bool Validate<T>(T arg)
        where T : class
    {
        var requiredProperties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => Attribute.IsDefined(x, typeof(RequiredMemberAttribute)));

        foreach (var requiredProperty in requiredProperties)
        {
            var propertyValue = requiredProperty.GetValue(arg);
            if (propertyValue is null)
            {
                throw new System.Exception($"Required property '{requiredProperty.Name}' was null");
            }
        }

        return true;
    }
}
