using System.Reflection;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class MessagePersistenceDbContext : DbContext
{
    /// <summary>
    /// The default database schema.
    /// </summary>
    public const string DefaultSchema = "messaging";

    public DbSet<StoreMessage> StoreMessages => Set<StoreMessage>();

    public MessagePersistenceDbContext(DbContextOptions<MessagePersistenceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
