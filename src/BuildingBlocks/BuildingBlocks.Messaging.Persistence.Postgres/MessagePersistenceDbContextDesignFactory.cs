using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using BuildingBlocks.Persistence.EfCore.Postgres;

namespace BuildingBlocks.Messaging.Persistence.Postgres;

public class MessagePersistenceDbContextDesignFactory : DbContextDesignFactoryBase<MessagePersistenceDbContext>
{
    public MessagePersistenceDbContextDesignFactory() : base("ConnectionStrings:PostgresMessaging")
    {
    }
}
