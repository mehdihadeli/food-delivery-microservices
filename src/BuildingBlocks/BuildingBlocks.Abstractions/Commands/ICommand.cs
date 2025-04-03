using Mediator;

namespace BuildingBlocks.Abstractions.Commands;

public interface ICommandBase;

public interface ICommand : ICommand<Unit>;

public interface ICommand<out TResponse> : IRequest<TResponse>, ICommandBase
    where TResponse : notnull;
