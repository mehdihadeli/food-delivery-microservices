using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using Mediator;

namespace BuildingBlocks.Core.Commands;

public class CommandBus(
    IMediator mediator,
    IServiceProvider serviceProvider,
    IMessageMetadataAccessor messageMetadataAccessor
) : AsyncCommandBus(serviceProvider, messageMetadataAccessor), ICommandBus
{
    public async Task<TResult> SendAsync<TResult>(
        Abstractions.Commands.ICommand<TResult> command,
        CancellationToken cancellationToken = default
    )
        where TResult : notnull
    {
        return await mediator.Send(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendAsync(Abstractions.Commands.ICommand command, CancellationToken cancellationToken = default)
    {
        await mediator.Send(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task ScheduleAsync(
        IInternalCommand internalCommandCommand,
        CancellationToken cancellationToken = default
    )
    {
        // to prevent cycle dependencies in MessagePersistenceService
        var messagePersistenceService = serviceProvider.GetRequiredService<IMessagePersistenceService>();

        await messagePersistenceService
            .AddInternalMessageAsync(internalCommandCommand, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task ScheduleAsync(
        IInternalCommand[] internalCommandCommands,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var internalCommandCommand in internalCommandCommands)
        {
            await ScheduleAsync(internalCommandCommand, cancellationToken).ConfigureAwait(false);
        }
    }
}
