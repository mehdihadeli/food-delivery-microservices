namespace BuildingBlocks.Abstractions.Persistence;

public interface ITestDataSeeder
{
    Task SeedAllAsync(CancellationToken cancellationToken);
    int Order { get; }
}
