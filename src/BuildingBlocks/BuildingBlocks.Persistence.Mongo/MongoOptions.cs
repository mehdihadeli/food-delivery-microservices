namespace BuildingBlocks.Persistence.Mongo;

public class MongoOptions
{
    public string ConnectionString { get; set; } = null!;
    public bool DisableTracing { get; set; }
    public bool DisableHealthChecks { get; set; }
}
