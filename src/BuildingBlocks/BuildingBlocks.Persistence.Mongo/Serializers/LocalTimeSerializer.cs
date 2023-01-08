using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace BuildingBlocks.Persistence.Mongo.Serializers;

public class LocalDateTimeSerializationProvider : IBsonSerializationProvider
{
    public IBsonSerializer GetSerializer(Type type)
    {
        return type == typeof(DateTime) ? DateTimeSerializer.LocalInstance : null;
    }
}
