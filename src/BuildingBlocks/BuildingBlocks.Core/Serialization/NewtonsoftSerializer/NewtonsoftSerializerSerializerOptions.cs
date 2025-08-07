using BuildingBlocks.Core.Serialization.NewtonsoftSerializer.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildingBlocks.Core.Serialization.NewtonsoftSerializer;

public static class NewtonsoftSerializerSerializerOptions
{
    public static JsonSerializerSettings DefaultSerializerSettings { get; } = CreateDefaultSerializerSettings();

    public static JsonSerializerSettings CreateDefaultSerializerSettings(bool camelCase = true, bool indented = false)
    {
        NamingStrategy strategy = camelCase ? new CamelCaseNamingStrategy() : new DefaultNamingStrategy();

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new ContractResolverWithPrivate { NamingStrategy = strategy },
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
