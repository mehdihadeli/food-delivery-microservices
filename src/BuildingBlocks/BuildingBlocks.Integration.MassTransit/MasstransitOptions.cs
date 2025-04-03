namespace BuildingBlocks.Integration.MassTransit;

public class MasstransitOptions
{
    public bool AutoConfigEndpoints { get; set; }
    public bool AutoConfigMessagesTopology { get; set; } = true;
    public bool ConfigureConsumeTopology { get; set; }
}
