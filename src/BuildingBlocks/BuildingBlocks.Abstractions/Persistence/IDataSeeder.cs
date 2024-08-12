namespace BuildingBlocks.Abstractions.Persistence;

public interface IDataSeeder
{
    int Order { get; }
    Task SeedAllAsync();
}
