namespace BuildingBlocks.Integration.Wolverine;

public class WolverineBusOptions
{
    public bool AutoConfigEndpoints { get; set; }
    public bool AutoConfigMessagesTopology { get; set; } = true;
    public bool ConfigureConsumeTopology { get; set; }
    public bool DisableHealthChecks { get; set; }
    public bool DisableTracing { get; set; }
    public bool EnableDurability { get; set; }
    public bool AutoProvisionPersistence { get; set; } = true;
    public bool UseDurableLocalQueues { get; set; } = true;
    public bool UseEntityFrameworkCoreTransactions { get; set; } = true;

    public string RabbitMQConnectionString { get; set; } = default!;
}
