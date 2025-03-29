using BuildingBlocks.Core.Messages.MessagePersistence;
using Core.Persistence.Postgres;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class NpgsqlMessagePersistenceConnectionFactory(string connectionString)
    : NpgsqlConnectionFactory(connectionString),
        IMessagePersistenceConnectionFactory;
