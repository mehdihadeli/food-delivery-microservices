using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildingBlocks.Core.Serialization.NewtonsoftSerializer;

internal class ContractResolverWithPrivate : DefaultContractResolver
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
