using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Types;

namespace BuildingBlocks.Core.Types;

public record MachineInstanceInfo : IMachineInstanceInfo
{
    public MachineInstanceInfo(Guid clientId, string clientGroup)
    {
        Guard.Against.NullOrEmpty(clientGroup, nameof(clientGroup));

        ClientId = clientId;
        ClientGroup = clientGroup;
    }

    public Guid ClientId { get; }
    public string ClientGroup { get; }

    internal static MachineInstanceInfo New() => new(Guid.NewGuid(), AppDomain.CurrentDomain.FriendlyName);
}
