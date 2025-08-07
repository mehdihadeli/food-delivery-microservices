using BuildingBlocks.Core.Messages.MessagePersistence;
using BuildingBlocks.Persistence.EfCore.Postgres;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class NpgsqlMessagePersistenceConnectionFactory(string? connectionString)
    : NpgsqlConnectionFactory(connectionString),
        IMessagePersistenceConnectionFactory;
