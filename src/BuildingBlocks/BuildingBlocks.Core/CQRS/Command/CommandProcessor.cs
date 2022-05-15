using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using MediatR;

namespace BuildingBlocks.Core.CQRS.Command;

public class CommandProcessor : ICommandProcessor
{
    private readonly IMediator _mediator;
    private readonly IMessagePersistenceService _messagePersistenceService;

    public CommandProcessor(IMediator mediator, IMessagePersistenceService messagePersistenceService)
    {
        _mediator = mediator;
        _messagePersistenceService = messagePersistenceService;
    }

    public Task<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
        where TResult : notnull
    {
        return _mediator.Send(command, cancellationToken);
    }

    public async Task ScheduleAsync(
        IInternalCommand internalCommandCommand,
        CancellationToken cancellationToken = default)
    {
        await _messagePersistenceService.AddInternalMessageAsync(internalCommandCommand, cancellationToken);
    }

    public async Task ScheduleAsync(
        IInternalCommand[] internalCommandCommands,
        CancellationToken cancellationToken = default)
    {
        foreach (var internalCommandCommand in internalCommandCommands)
        {
            await ScheduleAsync(internalCommandCommand, cancellationToken);
        }
    }
}
