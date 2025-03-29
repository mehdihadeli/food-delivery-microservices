namespace BuildingBlocks.Abstractions.Persistence;

public interface IMigrationSchema
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
