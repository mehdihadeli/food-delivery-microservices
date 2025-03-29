namespace BuildingBlocks.Abstractions.Persistence;

public interface IDataSeeder
{
    Task SeedAllAsync(CancellationToken cancellationToken);
    int Order { get; }
}
