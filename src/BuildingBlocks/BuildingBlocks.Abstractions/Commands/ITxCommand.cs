using BuildingBlocks.Abstractions.Persistence;

namespace BuildingBlocks.Abstractions.Commands;

public interface ITxCommand : ICommand, ITxRequest;

public interface ITxCommand<out T> : ICommand<T>, ITxRequest
    where T : notnull;
