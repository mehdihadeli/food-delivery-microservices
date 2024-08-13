using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Serialization;
using MemoryPack;

namespace BuildingBlocks.Serialization.MemoryPack;

public class MemoryPackMessageSerializer(MemoryPackSerializerOptions options)
    : MemoryPackObjectSerializer(options),
        IMessageSerializer
{
    public string Serialize(IEventEnvelope eventEnvelope)
    {
        throw new NotImplementedException();
    }

    public IEventEnvelope? Deserialize(string eventEnvelope)
    {
        throw new NotImplementedException();
    }

    public IEventEnvelope? Deserialize(string eventEnvelope, Type messageType)
    {
        throw new NotImplementedException();
    }
}
