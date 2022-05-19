using BuildingBlocks.Abstractions.CQRS.Commands;
using MediatR;

namespace BuildingBlocks.Core.CQRS.Commands;

public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    protected abstract Task<Unit> HandleCommandAsync(TCommand command, CancellationToken cancellationToken);

    public Task<Unit> Handle(TCommand request, CancellationToken cancellationToken)
    {
        return HandleCommandAsync(request, cancellationToken);
    }
}

public abstract class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : notnull
{
    protected abstract Task<TResponse> HandleCommandAsync(
        TCommand command,
        CancellationToken cancellationToken = default);

    public Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
    {
        return HandleCommandAsync(request, cancellationToken);
    }
}
