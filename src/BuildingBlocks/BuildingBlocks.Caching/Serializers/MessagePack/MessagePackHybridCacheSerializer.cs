using System.Buffers;
using MessagePack;
using Microsoft.Extensions.Caching.Hybrid;

namespace BuildingBlocks.Caching.Serializers.MessagePack;

public class MessagePackHybridCacheSerializer<T>(MessagePackSerializerOptions? options = null)
    : IHybridCacheSerializer<T>
{
    private readonly MessagePackSerializerOptions _options = options ?? MessagePackSerializer.DefaultOptions;

    public T Deserialize(ReadOnlySequence<byte> source)
    {
        // Deserialize the byte sequence into the target type T
        return MessagePackSerializer.Deserialize<T>(source.ToArray(), _options);
    }

    public void Serialize(T value, IBufferWriter<byte> target)
    {
        // Serialize the value into the target buffer
        var buffer = MessagePackSerializer.Serialize(value, _options);
        target.Write(buffer);
    }
}
