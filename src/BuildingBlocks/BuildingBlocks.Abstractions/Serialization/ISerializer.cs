namespace BuildingBlocks.Abstractions.Serialization;

public interface ISerializer
{
    string ContentType { get; }

    /// <summary>
    /// Serializes the given object into a string
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="camelCase"></param>
    /// <param name="indented"></param>
    /// <returns></returns>
    string Serialize(object obj, bool camelCase = true, bool indented = true);

    /// <summary>
    /// Deserialize the given string into a <see cref="T"/>
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="camelCase"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? Deserialize<T>(string payload, bool camelCase = true);

    /// <summary>
    /// Deserialize the given string into a object.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="type"></param>
    /// <param name="camelCase"></param>
    /// <returns></returns>
    object? Deserialize(string payload, Type type, bool camelCase = true);
}