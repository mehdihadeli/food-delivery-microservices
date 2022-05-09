using BuildingBlocks.Abstractions.CQRS.Command;

namespace BuildingBlocks.Core.CQRS.Command;

public abstract record TxInternalCommand : InternalCommand, ITxInternalCommand;
