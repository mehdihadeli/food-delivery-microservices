using BuildingBlocks.Core.Messaging.MessagePersistence;
using Core.Persistence.Postgres;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class NpgsqlMessagePersistenceConnectionFactory : NpgsqlConnectionFactory, IMessagePersistenceConnectionFactory
{
    public NpgsqlMessagePersistenceConnectionFactory(string connectionString) : base(connectionString)
    {
    }
}
