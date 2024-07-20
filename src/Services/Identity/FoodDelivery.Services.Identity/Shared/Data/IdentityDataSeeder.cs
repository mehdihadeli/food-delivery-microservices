using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Identity.Shared.Data;

public class IdentityDataSeeder : IDataSeeder
{
    public int Order => 2;

    public Task SeedAllAsync()
    {
        return Task.CompletedTask;
    }
}
