using BuildingBlocks.Abstractions.Persistence;

namespace BuildingBlocks.Abstractions.CQRS.Commands;

public interface ITxInternalCommand : IInternalCommand, ITxRequest
{
}
