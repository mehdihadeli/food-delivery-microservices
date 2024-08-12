using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Persistence.EfCore.Interceptors;
using FoodDelivery.Services.Catalogs.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FoodDelivery.Services.Catalogs.UnitTests.Common;

public static class DbContextFactory
{
    public static CatalogDbContext Create()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            // ref: https://andrewlock.net/series/using-strongly-typed-entity-ids-to-avoid-primitive-obsession/
            .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector<long>>()
            .AddInterceptors(new AuditInterceptor(), new SoftDeleteInterceptor(), new ConcurrencyInterceptor())
            .Options;

        var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    public static async Task Destroy(CatalogDbContext context)
    {
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }
}
