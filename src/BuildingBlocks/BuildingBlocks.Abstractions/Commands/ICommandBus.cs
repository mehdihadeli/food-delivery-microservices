namespace BuildingBlocks.Abstractions.Commands;

public interface ICommandBus : IAsyncCommandBus
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        where TResult : notnull;

    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);

    Task ScheduleAsync(IInternalCommand internalCommandCommand, CancellationToken cancellationToken = default);
    Task ScheduleAsync(IInternalCommand[] internalCommandCommands, CancellationToken cancellationToken = default);
}
