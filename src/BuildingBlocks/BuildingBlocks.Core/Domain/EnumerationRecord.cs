using System.Reflection;

namespace BuildingBlocks.Core.Domain;

// Ref: https://josef.codes/enumeration-class-in-c-sharp-using-records/
public abstract record EnumerationRecord<T>(int Value, string DisplayName) : IComparable<T>
    where T : EnumerationRecord<T>
{
    private static readonly Lazy<Dictionary<int, T>> _allItems;
    private static readonly Lazy<Dictionary<string, T>> _allItemsByName;

    static EnumerationRecord()
    {
        _allItems = new Lazy<Dictionary<int, T>>(() =>
        {
            return typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(x => x.FieldType == typeof(T))
                .Select(x => x.GetValue(null))
                .Cast<T>()
                .ToDictionary(x => x.Value, x => x);
        });
        _allItemsByName = new Lazy<Dictionary<string, T>>(() =>
        {
            var items = new Dictionary<string, T>(_allItems.Value.Count);
            foreach (var item in _allItems.Value)
            {
                if (!items.TryAdd(item.Value.DisplayName, item.Value))
                {
                    throw new System.Exception(
                        $"DisplayName needs to be unique. '{item.Value.DisplayName}' already exists");
                }
            }
            return items;
        });
    }

    public override string ToString() => DisplayName;

    public static IEnumerable<T> GetAll()
    {
        return _allItems.Value.Values;
    }

    public static int AbsoluteDifference(EnumerationRecord<T> firstValue, EnumerationRecord<T> secondValue)
    {
        return Math.Abs(firstValue.Value - secondValue.Value);
    }

    public static T FromValue(int value)
    {
        if (_allItems.Value.TryGetValue(value, out var matchingItem))
        {
            return matchingItem;
        }
        throw new InvalidOperationException($"'{value}' is not a valid value in {typeof(T)}");
    }

    public static T FromDisplayName(string displayName)
    {
        if (_allItemsByName.Value.TryGetValue(displayName, out var matchingItem))
        {
            return matchingItem;
        }
        throw new InvalidOperationException($"'{displayName}' is not a valid display name in {typeof(T)}");
    }

    public int CompareTo(T? other) => Value.CompareTo(other!.Value);
}
