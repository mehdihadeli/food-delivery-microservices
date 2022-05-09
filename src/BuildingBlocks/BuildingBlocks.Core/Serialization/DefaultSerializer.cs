using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Abstractions.Serialization;

namespace BuildingBlocks.Core.Serialization;

public class DefaultSerializer : ISerializer
{
    public string ContentType => "application/json";

    public string Serialize(object obj, bool camelCase = true, bool indented = true)
    {
        return JsonSerializer.Serialize(obj, CreateSerializerSettings(camelCase, indented));
    }

    public T? Deserialize<T>(string payload, bool camelCase = true)
    {
        return JsonSerializer.Deserialize<T>(payload, CreateSerializerSettings(camelCase));
    }

    public object? Deserialize(string payload, Type type, bool camelCase = true)
    {
        return JsonSerializer.Deserialize(payload, type, CreateSerializerSettings(camelCase));
    }

    private JsonSerializerOptions CreateSerializerSettings(bool camelCase = true, bool indented = false)
    {
        var settings = new JsonSerializerOptions
        {
            WriteIndented = indented,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null
        };

        return settings;
    }
}
