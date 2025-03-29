namespace BuildingBlocks.Abstractions.Persistence;

public interface IDataSeederManager
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
