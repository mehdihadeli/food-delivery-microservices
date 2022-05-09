using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;

public class InMemoryMessagePersistenceContext : DbContext
{
    public InMemoryMessagePersistenceContext(DbContextOptions<InMemoryMessagePersistenceContext> options) : base(options)
    {
    }

    public DbSet<StoreMessage> StoreMessages => Set<StoreMessage>();
}