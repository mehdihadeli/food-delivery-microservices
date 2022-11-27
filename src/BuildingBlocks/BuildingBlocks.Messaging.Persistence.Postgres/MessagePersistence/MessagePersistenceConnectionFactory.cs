using BuildingBlocks.Core.Messaging.MessagePersistence;
using Core.Persistence.Postgres;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class MessagePersistenceConnectionFactory : EfNpgsqlConnectionFactory, IMessagePersistenceConnectionFactory
{
    public MessagePersistenceConnectionFactory(string connectionString) : base(connectionString)
    {
    }
}
