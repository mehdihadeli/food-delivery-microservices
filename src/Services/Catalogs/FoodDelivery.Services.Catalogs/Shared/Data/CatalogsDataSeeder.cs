using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CatalogsDataSeeder : IDataSeeder
{
    public Task SeedAllAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public int Order => 5;
}
