namespace BuildingBlocks.Abstractions.Serialization;

public interface ISerializer
{
    string ContentType { get; }

    /// <summary>
    ///     Serializes the given object into a string.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    string Serialize(object obj);

    /// <summary>
    ///     Deserialize the given string into a <see cref="T" />.
    /// </summary>
    /// <param name="payload"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? Deserialize<T>(string payload);

    /// <summary>
    ///     Deserialize the given string into an object.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    object? Deserialize(string payload, Type type);
}
