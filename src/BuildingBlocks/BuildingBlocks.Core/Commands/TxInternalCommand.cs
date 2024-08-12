using BuildingBlocks.Abstractions.Commands;

namespace BuildingBlocks.Core.Commands;

public abstract record TxInternalCommand : InternalCommand, ITxInternalCommand;
