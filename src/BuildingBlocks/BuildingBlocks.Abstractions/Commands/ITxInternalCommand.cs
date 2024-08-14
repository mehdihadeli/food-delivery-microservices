using BuildingBlocks.Abstractions.Persistence;

namespace BuildingBlocks.Abstractions.Commands;

public interface ITxInternalCommand : IInternalCommand, ITxRequest;
