namespace BuildingBlocks.Abstractions.CQRS.Commands;

public interface ICommandProcessor
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        where TResult : class;

    Task SendAsync<TRequest>(TRequest command, CancellationToken cancellationToken = default)
        where TRequest : ICommand;

    Task ScheduleAsync(IInternalCommand internalCommandCommand, CancellationToken cancellationToken = default);
    Task ScheduleAsync(IInternalCommand[] internalCommandCommands, CancellationToken cancellationToken = default);
}
