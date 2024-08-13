using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Core.Serialization.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildingBlocks.Core.Serialization;

internal static class DependencyInjectionExtensions
{
    internal static void AddDefaultSerializer(
        this IServiceCollection services,
        Action<JsonSerializerSettings>? configuration = null
    )
    {
        var defaultSettings = CreateDefaultSerializerSettings();
        configuration?.Invoke(defaultSettings);

        services.AddTransient<ISerializer, NewtonsoftObjectSerializer>();
        services.AddTransient<IMessageSerializer, NewtonsoftMessageSerializer>();
    }

    private static JsonSerializerSettings CreateDefaultSerializerSettings(bool camelCase = true, bool indented = false)
    {
        NamingStrategy strategy = camelCase ? new CamelCaseNamingStrategy() : new DefaultNamingStrategy();

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new ContractResolverWithPrivate { NamingStrategy = strategy }
        };

        if (indented)
        {
            settings.Formatting = Formatting.Indented;
        }

        // for handling private constructor
        settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        settings.Converters.Add(new DateOnlyConverter());

        return settings;
    }
}
