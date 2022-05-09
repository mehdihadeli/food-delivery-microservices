namespace BuildingBlocks.Core.Extensions;

public static class DictionaryExtensions
{
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            return false;
        }

        dictionary.Add(key, value);
        return true;
    }

    public static bool AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary.Remove(key);
        }

        return dictionary.TryAdd(key, value);
    }


    public static object? Get(this IDictionary<string, object?> dictionary, string key)
    {
        dictionary.TryGetValue(key, out object? val);

        return val;
    }

    public static TValue? Get<TValue>(this IDictionary<string, object?> dictionary, string key)
        where TValue : class
    {
        dictionary.TryGetValue(key, out object? val);

        return val as TValue;
    }
}
