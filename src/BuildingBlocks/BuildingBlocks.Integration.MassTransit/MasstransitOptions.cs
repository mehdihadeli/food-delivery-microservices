namespace BuildingBlocks.Integration.MassTransit;

public class MasstransitOptions
{
    public bool AutoConfigEndpoints { get; set; }
    public bool AutoConfigMessagesTopology { get; set; } = true;
    public bool ConfigureConsumeTopology { get; set; }
    public bool DisableHealthChecks { get; set; }
    public bool DisableTracing { get; set; }

    public string RabbitMQConnectionString { get; set; } = default!;

    public MasstransitMessagingProvider MasstransitMessagingProvider { get; set; } =
        MasstransitMessagingProvider.RabbitMQ;
}
