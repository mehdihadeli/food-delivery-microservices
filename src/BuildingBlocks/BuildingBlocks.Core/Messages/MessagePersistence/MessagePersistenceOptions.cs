namespace BuildingBlocks.Core.Messages.MessagePersistence;

public class MessagePersistenceOptions
{
    public int? Interval { get; set; } = 30;
    public string ConnectionString { get; set; } = default!;
    public bool Enabled { get; set; } = true;
    public bool UseIdempotentMessageHandling { get; set; } = true;
    public bool UseDistributedLock { get; set; }
    public bool UsePartitioning { get; set; }
    public string? MigrationAssembly { get; set; }
}
