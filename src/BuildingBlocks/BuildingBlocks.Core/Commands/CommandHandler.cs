using Mediator;
using ICommand = BuildingBlocks.Abstractions.Commands.ICommand;

namespace BuildingBlocks.Core.Commands;

public abstract class CommandHandler<TCommand> : Abstractions.Commands.ICommandHandler<TCommand>
    where TCommand : ICommand
{
    protected abstract ValueTask<Unit> HandleCommandAsync(TCommand command, CancellationToken cancellationToken);

    public ValueTask<Unit> Handle(TCommand request, CancellationToken cancellationToken)
    {
        return HandleCommandAsync(request, cancellationToken);
    }
}

public abstract class CommandHandler<TCommand, TResponse> : Abstractions.Commands.ICommandHandler<TCommand, TResponse>
    where TCommand : Abstractions.Commands.ICommand<TResponse>
    where TResponse : notnull
{
    protected abstract ValueTask<TResponse> HandleCommandAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    );

    public ValueTask<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
    {
        return HandleCommandAsync(request, cancellationToken);
    }
}
