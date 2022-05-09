using System.Collections;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BuildingBlocks.Core.Extensions;

// https://khalidabuhakmeh.com/read-and-convert-querycollection-values-in-aspnet
public static class QueryCollectionExtensions
{
    public static IEnumerable<T> All<T>(
        this IQueryCollection collection,
        string key)
    {
        List<T> values = new List<T>();
        if (collection.TryGetValue(key, out var results))
        {
            foreach (var s in results)
            {
                try
                {
                    var result = (T)Convert.ChangeType(s, typeof(T));
                    values.Add(result);
                }
                catch (System.Exception)
                {
                    // conversion failed
                    // skip value
                }
            }
        }

        return values;
    }

    public static T Get<T>(
        this IQueryCollection collection,
        string key,
        T @default = default,
        ParameterPick option = ParameterPick.First)
    {
        var values = All<T>(collection, key);
        var value = @default;

        if (values.Any())
        {
            value = option switch
            {
                ParameterPick.First => values.FirstOrDefault(),
                ParameterPick.Last => values.LastOrDefault(),
                _ => value
            };
        }

        return value ?? @default;
    }

    public static T GetCollection<T>(
        this IQueryCollection collection,
        string key,
        T @default = default)
        where T : IEnumerable
    {
        var type = typeof(T).GetGenericArguments()[0];
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(type);
        dynamic values = Activator.CreateInstance(constructedListType);

        if (collection.TryGetValue(key, out var results))
        {
            foreach (var s in results)
            {
                try
                {
                    if (s.IsValidJson())
                    {
                        dynamic result = JsonConvert.DeserializeObject(s, type);
                        values.Add(result);
                    }
                    else
                    {
                        dynamic result = Convert.ChangeType(s, type);
                        values.Add(result);
                    }
                }
                catch (System.Exception)
                {
                    // conversion failed
                    // skip value
                }
            }
        }
        else
        {
            return @default;
        }

        return values;
    }
}
