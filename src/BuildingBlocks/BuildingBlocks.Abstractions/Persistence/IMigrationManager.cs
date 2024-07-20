namespace BuildingBlocks.Abstractions.Persistence;

public interface IMigrationManager
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
