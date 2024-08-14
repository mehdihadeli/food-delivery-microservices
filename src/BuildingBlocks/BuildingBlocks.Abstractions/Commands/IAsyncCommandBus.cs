namespace BuildingBlocks.Abstractions.Commands;

public interface IAsyncCommandBus
{
    public Task SendExternalAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : IAsyncCommand;
}
