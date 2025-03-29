namespace BuildingBlocks.Core.Types;

using BuildingBlocks.Core.Reflection;

public static class TypeMapper
{
    private static readonly Dictionary<Type, HashSet<string>> _typeToNames = new();
    private static readonly Dictionary<string, Type?> _nameToType = new();

    /// <summary>
    /// Adds a type mapping using the class's simple name.
    /// </summary>
    /// <param name="type">The type to be mapped</param>
    /// <returns>The simple name that was added</returns>
    /// <exception cref="ArgumentNullException">If type is null</exception>
    /// <exception cref="ArgumentException">If simple name is already mapped to a different type</exception>
    public static string AddShortTypeName(Type? type)
    {
        ArgumentNullException.ThrowIfNull(type);

        string simpleName = type.Name;
        AddTypeNameInternal(type, simpleName);

        return simpleName;
    }

    /// <summary>
    /// Adds a type mapping with a custom type name.
    /// </summary>
    /// <param name="type">The type to be mapped</param>
    /// <param name="typeName">The custom name for the type</param>
    /// <returns>The custom type name that was added</returns>
    /// <exception cref="ArgumentNullException">If type or typeName is null</exception>
    /// <exception cref="ArgumentException">If name is already mapped to a different type</exception>
    public static string AddShortTypeName(Type type, string typeName)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (string.IsNullOrEmpty(typeName))
        {
            throw new ArgumentException("TypeName cannot be null or empty", nameof(typeName));
        }

        AddTypeNameInternal(type, typeName);

        return typeName;
    }

    /// <summary>
    /// Adds a type mapping with the full qualified type name.
    /// </summary>
    /// <param name="type">The type to be mapped</param>
    /// <returns>The full qualified name that was added</returns>
    /// <exception cref="ArgumentNullException">If type is null</exception>
    public static string AddFullTypeName(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        string fullName = type.FullName;
        AddTypeNameInternal(type, fullName);

        return fullName;
    }

    private static void AddTypeNameInternal(Type type, string typeName)
    {
        // Check if typeName is already mapped to a different type
        if (_nameToType.TryGetValue(typeName, out Type? existingType) && existingType != null && existingType != type)
        {
            throw new ArgumentException($"TypeName '{typeName}' is already mapped to type {existingType.FullName}");
        }

        // Add to both dictionaries
        if (!_typeToNames.TryGetValue(type, out HashSet<string>? value))
        {
            value = new HashSet<string>();
            _typeToNames[type] = value;
        }

        value.Add(typeName);
        _nameToType[typeName] = type;
    }

    /// <summary>
    /// Gets the type based on the type name
    /// </summary>
    /// <param name="typeName">The name to look up</param>
    /// <returns>The corresponding Type object</returns>
    /// <exception cref="ArgumentException">If typeName is null or empty</exception>
    /// <exception cref="Exception">If type cannot be found</exception>
    public static Type? GetType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            throw new ArgumentException("TypeName cannot be null or empty", nameof(typeName));
        }

        // First try to get from our mapped types
        if (_nameToType.TryGetValue(typeName, out Type? type))
        {
            return type;
        }

        // If not found, try to get from reflection
        type = ReflectionUtilities.GetFirstMatchingTypeFromCurrentDomainAssemblies(typeName);

        if (type == null)
        {
            throw new System.Exception($"Type map for '{typeName}' wasn't found!");
        }

        return type;
    }

    /// <summary>
    /// Gets all registered type names for a given type.
    /// </summary>
    /// <param name="type">The type to look up</param>
    /// <returns>Set of all registered type names, or empty set if none found</returns>
    /// <exception cref="ArgumentNullException">If type is null</exception>
    public static IReadOnlySet<string> GetAllTypeNames(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return _typeToNames.TryGetValue(type, out var names) ? names : new HashSet<string>();
    }
}
