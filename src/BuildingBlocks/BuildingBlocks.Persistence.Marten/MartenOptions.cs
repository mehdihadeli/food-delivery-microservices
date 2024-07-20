using Marten.Events.Daemon.Resiliency;

namespace BuildingBlocks.Persistence.Marten;

public class MartenOptions
{
    private const string DefaultSchema = "public";

    public string ConnectionString { get; set; } = default!;
    public string WriteModelSchema { get; set; } = DefaultSchema;
    public string ReadModelSchema { get; set; } = DefaultSchema;
    public bool ShouldRecreateDatabase { get; set; }
    public DaemonMode DaemonMode { get; set; } = DaemonMode.Disabled;
    public bool UseMetadata { get; set; } = true;
}
