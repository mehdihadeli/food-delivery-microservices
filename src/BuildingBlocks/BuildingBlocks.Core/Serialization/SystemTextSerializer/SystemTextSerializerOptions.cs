using System.Text.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Core.Serialization.SystemTextSerializer;

public static class SystemTextJsonSerializerOptions
{
    public static JsonSerializerOptions DefaultSerializerOptions { get; } = CreateDefaultSerializerOptions();

    public static JsonSerializerOptions CreateDefaultSerializerOptions(bool camelCase = true, bool indented = false)
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            // Equivalent to ReferenceLoopHandling.Ignore
            ReferenceHandler = ReferenceHandler.IgnoreCycles,

            WriteIndented = indented,
            PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null,
        };

        // For DateOnly support (similar to your DateOnlyConverter)
        options.Converters.Add(new JsonDateOnlyConverter());

        return options;
    }
}
