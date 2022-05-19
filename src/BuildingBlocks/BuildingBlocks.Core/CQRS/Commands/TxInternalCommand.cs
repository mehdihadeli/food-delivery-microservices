using BuildingBlocks.Abstractions.CQRS.Commands;

namespace BuildingBlocks.Core.CQRS.Commands;

public abstract record TxInternalCommand : InternalCommand, ITxInternalCommand;
