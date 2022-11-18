using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Core.Extensions;

/// <summary>
/// Static helper class for <see cref="IConfiguration"/>.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Attempts to bind the <typeparamref name="TModel"/> instance to configuration values by matching property names against configuration keys.
    /// </summary>
    /// <typeparam name="TModel">The given bind model.</typeparam>
    /// <param name="configuration">The configuration instance to bind.</param>
    /// <param name="section">The configuration section</param>
    /// <returns>The new instance of <typeparamref name="TModel"/>.</returns>
    public static TModel? GetOptions<TModel>(this IConfiguration configuration, string section)
        where TModel : new()
    {
        // note: with using Get<>() if there is no configuration in appsettings it just returns default value (null) for the configuration type
        // but if we use Bind() we can pass a instantiated type with its default value (for example in its property initialization) to bind method for binding configurations from appsettings
        // https://www.twilio.com/blog/provide-default-configuration-to-dotnet-applications
        var options = new TModel();

        var optionsSection = configuration.GetSection(section);
        optionsSection.Bind(options);

        return options;
    }

    /// <summary>
    /// Attempts to bind the <typeparamref name="TModel"/> instance to configuration values by matching property names against configuration keys.
    /// </summary>
    /// <typeparam name="TModel">The given bind model.</typeparam>
    /// <param name="configuration">The configuration instance to bind.</param>
    /// <returns>The new instance of <typeparamref name="TModel"/>.</returns>
    public static TModel? GetOptions<TModel>(this IConfiguration configuration)
        where TModel : new()
    {
        return GetOptions<TModel>(configuration, typeof(TModel).Name);
    }
}
