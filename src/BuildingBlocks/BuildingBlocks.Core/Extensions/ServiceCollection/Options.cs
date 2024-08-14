using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.Extensions.ServiceCollection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfigurationOptions<T>(
        this IServiceCollection services,
        string? key = null,
        Action<T>? configurator = null
    )
        where T : class
    {
        var optionBuilder = services.AddOptions<T>().BindConfiguration(key ?? typeof(T).Name);

        if (configurator is not null)
        {
            optionBuilder = optionBuilder.Configure(configurator);
        }

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

        return AddValidatedOptions(services, key, RequiredConfigurationValidator.Validate, configurator);
    }

    public static IServiceCollection AddValidationOptions<T>(
        this IServiceCollection services,
        string? key = null,
        Action<T>? configurator = null
    )
        where T : class
    {
        return AddValidatedOptions(
            services,
            key ?? typeof(T).Name,
            RequiredConfigurationValidator.Validate,
            configurator
        );
    }

    public static IServiceCollection AddValidatedOptions<T>(
        this IServiceCollection services,
        string? key = null,
        Func<T, bool>? validator = null,
        Action<T>? configurator = null
    )
        where T : class
    {
        if (validator is null)
        {
            validator = RequiredConfigurationValidator.Validate;
        }

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options
        // https://thecodeblogger.com/2021/04/21/options-pattern-in-net-ioptions-ioptionssnapshot-ioptionsmonitor/
        // https://code-maze.com/aspnet-configuration-options/
        // https://code-maze.com/aspnet-configuration-options-validation/
        // https://dotnetdocs.ir/Post/42/difference-between-ioptions-ioptionssnapshot-and-ioptionsmonitor
        // https://andrewlock.net/adding-validation-to-strongly-typed-configuration-objects-in-dotnet-6/

        var optionBuilder = services.AddOptions<T>().BindConfiguration(key ?? typeof(T).Name);

        if (configurator is not null)
        {
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#configure-options-with-a-delegate
            optionBuilder = optionBuilder.Configure(configurator);
        }

        optionBuilder.Validate(validator);

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
