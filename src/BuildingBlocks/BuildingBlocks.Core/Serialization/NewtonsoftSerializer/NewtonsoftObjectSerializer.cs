using BuildingBlocks.Abstractions.Serialization;
using Newtonsoft.Json;

namespace BuildingBlocks.Core.Serialization.NewtonsoftSerializer;

public class NewtonsoftObjectSerializer(JsonSerializerSettings settings) : ISerializer
{
    public string ContentType => "application/json";

    public string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, settings);
    }

    public T? Deserialize<T>(string payload)
    {
        return JsonConvert.DeserializeObject<T>(payload, settings);
    }

    public object? Deserialize(string payload, Type? type)
    {
        return JsonConvert.DeserializeObject(payload, type, settings);
    }
}
