using BuildingBlocks.Abstractions.Persistence;

namespace BuildingBlocks.Abstractions.CQRS.Command;

public interface ITxInternalCommand : IInternalCommand, ITxRequest
{
}
