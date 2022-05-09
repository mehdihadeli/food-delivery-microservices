using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Scheduler;
using MediatR;

namespace BuildingBlocks.Core.CQRS.Command;

public class CommandProcessor : ICommandProcessor
{
    private readonly IMediator _mediator;
    private readonly ICommandScheduler _commandScheduler;

    public CommandProcessor(
        IMediator mediator,
        ICommandScheduler commandScheduler
    )
    {
        _mediator = mediator;
        _commandScheduler = commandScheduler;
    }

    public Task<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        return _mediator.Send(command, cancellationToken);
    }

    public Task ScheduleAsync(IInternalCommand internalCommandCommand, CancellationToken cancellationToken = default)
    {
        return _commandScheduler.ScheduleAsync(internalCommandCommand, cancellationToken);
    }

    public Task ScheduleAsync(IInternalCommand[] internalCommandCommands, CancellationToken cancellationToken = default)
    {
        return _commandScheduler.ScheduleAsync(internalCommandCommands, cancellationToken);
    }
}
