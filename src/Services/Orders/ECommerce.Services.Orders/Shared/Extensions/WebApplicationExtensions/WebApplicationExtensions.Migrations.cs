using ECommerce.Services.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Orders.Shared.Extensions.WebApplicationExtensions;

public static partial class WebApplicationExtensions
{
    public static async Task ApplyDatabaseMigrations(this WebApplication app)
    {
        var configuration = app.Services.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            using var serviceScope = app.Services.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<OrdersDbContext>();

            app.Logger.LogInformation("Updating catalog database...");

            await dbContext.Database.MigrateAsync();

            app.Logger.LogInformation("Updated catalog database");
        }
    }
}
