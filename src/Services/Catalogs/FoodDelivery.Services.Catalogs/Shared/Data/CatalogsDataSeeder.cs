using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CatalogsDataSeeder : IDataSeeder
{
    public int Order => 5;

    public Task SeedAllAsync()
    {
        return Task.CompletedTask;
    }
}
