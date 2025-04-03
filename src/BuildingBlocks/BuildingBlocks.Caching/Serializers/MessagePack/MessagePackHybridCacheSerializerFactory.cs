using MessagePack;
using Microsoft.Extensions.Caching.Hybrid;

namespace BuildingBlocks.Caching.Serializers.MessagePack;

public class MessagePackHybridCacheSerializerFactory(MessagePackSerializerOptions? options = null)
    : IHybridCacheSerializerFactory
{
    private readonly MessagePackSerializerOptions _options = options ?? MessagePackSerializer.DefaultOptions;

    public bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer)
    {
        // Try to create a serializer for the type T.
        serializer = new MessagePackHybridCacheSerializer<T>(_options);
        return true;
    }
}
