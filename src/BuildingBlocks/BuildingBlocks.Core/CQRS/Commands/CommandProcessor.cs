using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using MediatR;

namespace BuildingBlocks.Core.CQRS.Commands;

public class CommandProcessor : ICommandProcessor
{
    private readonly IMediator _mediator;
    private readonly IMessagePersistenceService _messagePersistenceService;

    public CommandProcessor(IMediator mediator, IMessagePersistenceService messagePersistenceService)
    {
        _mediator = mediator;
        _messagePersistenceService = messagePersistenceService;
    }

    public async Task<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        return await _mediator.Send(command, cancellationToken);
    }

    public async Task SendAsync<TRequest>(TRequest command, CancellationToken cancellationToken = default)
        where TRequest : ICommand
    {
        await _mediator.Send(command, cancellationToken);
    }

    public async Task ScheduleAsync(
        IInternalCommand internalCommandCommand,
        CancellationToken cancellationToken = default
    )
    {
        await _messagePersistenceService.AddInternalMessageAsync(internalCommandCommand, cancellationToken);
    }

    public async Task ScheduleAsync(
        IInternalCommand[] internalCommandCommands,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var internalCommandCommand in internalCommandCommands)
        {
            await ScheduleAsync(internalCommandCommand, cancellationToken);
        }
    }
}
