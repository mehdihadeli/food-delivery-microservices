using BuildingBlocks.Abstractions.Persistence;
using Mediator;

namespace BuildingBlocks.Abstractions.Commands;

public interface ITxCommand : ITxCommand<Unit>;

public interface ITxCommand<out T> : ICommand<T>, ITxRequest
    where T : notnull;
