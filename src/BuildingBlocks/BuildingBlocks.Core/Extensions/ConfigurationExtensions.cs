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
        var model = configuration.GetSection(section).Get<TModel>();

        return model;
    }
}
