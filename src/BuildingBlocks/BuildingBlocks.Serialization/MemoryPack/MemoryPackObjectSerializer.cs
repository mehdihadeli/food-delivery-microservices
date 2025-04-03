using System.Text;
using BuildingBlocks.Abstractions.Serialization;
using MemoryPack;

namespace BuildingBlocks.Serialization.MemoryPack;

public class MemoryPackObjectSerializer(MemoryPackSerializerOptions options) : ISerializer
{
    public string ContentType => "binary/memorypack";

    public string Serialize(object obj)
    {
        return Encoding.UTF8.GetString(MemoryPackSerializer.Serialize(obj, options));
    }

    public T? Deserialize<T>(string payload)
    {
        ReadOnlySpan<byte> byteSpan = StringToReadOnlySpan(payload);

        return MemoryPackSerializer.Deserialize<T>(byteSpan, options);
    }

    public object? Deserialize(string payload, Type? type)
    {
        ReadOnlySpan<byte> byteSpan = StringToReadOnlySpan(payload);

        return MemoryPackSerializer.Deserialize(type, byteSpan, options);
    }

    private static ReadOnlySpan<byte> StringToReadOnlySpan(string input)
    {
        // Choose the encoding
        Encoding encoding = Encoding.UTF8;

        // Convert the string to a byte array
        byte[] byteArray = encoding.GetBytes(input);

        // Return a ReadOnlySpan<byte> from the byte array
        return new ReadOnlySpan<byte>(byteArray);
    }
}
