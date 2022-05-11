namespace BuildingBlocks.Abstractions.Types;

public interface IMachineInstanceInfo
{
    string ClientGroup { get; }
    Guid ClientId { get; }
}
