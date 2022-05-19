namespace BuildingBlocks.Abstractions.CQRS.Commands;

public interface ICommandProcessor
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        where TResult : notnull;

    Task ScheduleAsync(IInternalCommand internalCommandCommand, CancellationToken cancellationToken = default);
    Task ScheduleAsync(IInternalCommand[] internalCommandCommands, CancellationToken cancellationToken = default);
}
