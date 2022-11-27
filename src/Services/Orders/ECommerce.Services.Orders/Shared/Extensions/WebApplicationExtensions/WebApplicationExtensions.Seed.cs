using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Web.Extensions;

namespace ECommerce.Services.Orders.Shared.Extensions.WebApplicationExtensions;

public static partial class WebApplicationExtensions
{
    public static async Task SeedData(this WebApplication app)
    {
        if (!app.Environment.IsTest())
        {
            // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
            // https://www.michalbialecki.com/2020/07/20/adding-entity-framework-core-5-migrations-to-net-5-project/
            using var serviceScope = app.Services.CreateScope();
            var seeders = serviceScope.ServiceProvider.GetServices<IDataSeeder>();

            foreach (var seeder in seeders)
            {
                app.Logger.LogInformation("Seeding '{Seed}' started...", seeder.GetType().Name);
                await seeder.SeedAllAsync();
                app.Logger.LogInformation("Seeding '{Seed}' ended...", seeder.GetType().Name);
            }
        }
    }
}
