using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Types;

public record MachineInstanceInfo : IMachineInstanceInfo
{
    public MachineInstanceInfo(Guid clientId, string? clientGroup)
    {
        clientGroup.NotBeNullOrWhiteSpace();
        clientId.NotBeEmpty();

        ClientId = clientId;
        ClientGroup = clientGroup;
    }

    public Guid ClientId { get; }
    public string ClientGroup { get; }

    internal static MachineInstanceInfo New() => new(Guid.NewGuid(), AppDomain.CurrentDomain.FriendlyName);
}
