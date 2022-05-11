using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Abstractions.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonProperty = Newtonsoft.Json.Serialization.JsonProperty;

namespace BuildingBlocks.Core.Serialization;

public class DefaultSerializer : ISerializer
{
    public string ContentType => "application/json";

    public string Serialize(object obj, bool camelCase = true, bool indented = true)
    {
        return JsonConvert.SerializeObject(obj, CreateSerializerSettings(camelCase, indented));
    }

    public T? Deserialize<T>(string payload, bool camelCase = true)
    {
        return JsonConvert.DeserializeObject<T>(payload, CreateSerializerSettings(camelCase));
    }

    public object? Deserialize(string payload, Type type, bool camelCase = true)
    {
        return JsonConvert.DeserializeObject(payload, type, CreateSerializerSettings(camelCase));
    }


    protected JsonSerializerSettings? CreateSerializerSettings(bool camelCase = true, bool indented = false)
    {
        var settings = new JsonSerializerSettings {ContractResolver = new ContractResolverWithPrivate()};

        if (indented)
        {
            settings.Formatting = Formatting.Indented;
        }

        // for handling private constructor
        settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        return settings;
    }

    private class ContractResolverWithPrivate : CamelCasePropertyNamesContractResolver
    {
        // http://danielwertheim.se/json-net-private-setters/
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }
}
