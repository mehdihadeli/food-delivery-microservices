namespace BuildingBlocks.Abstractions.Persistence;

public interface ITestDataSeeder
{
    int Order { get; }
    Task SeedAllAsync();
}
