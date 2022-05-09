namespace BuildingBlocks.Abstractions.Messaging;

public class ContextItems
{
    private readonly Dictionary<string, object?> _items = new();

    public ContextItems AddItem(string key, object? value)
    {
        _items.TryAdd(key, value);
        return this;
    }

    public T? TryGetItem<T>(string key)
    {
        if (_items.TryGetValue(key, out var result))
        {
            return result is T type ? type : default;
        }

        return default;
    }
}
