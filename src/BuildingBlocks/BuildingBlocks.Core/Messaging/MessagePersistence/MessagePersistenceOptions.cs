namespace BuildingBlocks.Core.Messaging.MessagePersistence;

public class MessagePersistenceOptions
{
    public int? Interval { get; set; } = 30;
    public string ConnectionString { get; set; } = default!;
    public bool Enabled { get; set; } = true;
    public string? MigrationAssembly { get; set; } = null!;
}
