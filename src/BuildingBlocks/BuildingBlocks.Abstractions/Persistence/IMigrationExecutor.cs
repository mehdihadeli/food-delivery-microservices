namespace BuildingBlocks.Abstractions.Persistence;

public interface IMigrationExecutor
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
