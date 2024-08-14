using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using MediatR;

namespace BuildingBlocks.Core.Commands;

public class CommandBus(IMediator mediator, IMessagePersistenceService messagePersistenceService) : ICommandBus
{
    public async Task<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default
    )
        where TResult : class
    {
        return await mediator.Send(command, cancellationToken);
    }

    public async Task SendAsync<TRequest>(TRequest command, CancellationToken cancellationToken = default)
        where TRequest : ICommand
    {
        await mediator.Send(command, cancellationToken);
    }

    public async Task ScheduleAsync(
        IInternalCommand internalCommandCommand,
        CancellationToken cancellationToken = default
    )
    {
        await messagePersistenceService.AddInternalMessageAsync(internalCommandCommand, cancellationToken);
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
