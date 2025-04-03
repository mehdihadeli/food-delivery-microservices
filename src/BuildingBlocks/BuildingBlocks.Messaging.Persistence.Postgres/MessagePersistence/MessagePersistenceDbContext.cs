using System.Reflection;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class MessagePersistenceDbContext : DbContext
{
    /// <summary>
    /// The default database schema.
    /// </summary>
    public const string DefaultSchema = "messaging";

    public DbSet<PersistMessage> PersistMessages => Set<PersistMessage>();

    public MessagePersistenceDbContext(DbContextOptions<MessagePersistenceDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
