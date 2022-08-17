namespace BuildingBlocks.Abstractions.Persistence;

public interface IDataSeeder
{
    Task SeedAllAsync();
    int Order { get; }
}
