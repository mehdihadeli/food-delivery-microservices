using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo.Extensions;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddStorage(this WebApplicationBuilder builder)
    {
        AddPostgresWriteStorage(builder);
        AddMongoReadStorage(builder);

        return builder;
    }

    private static void AddPostgresWriteStorage(WebApplicationBuilder builder)
    {
        var option = builder.Configuration.BindOptions<PostgresOptions>();
        if (option.UseInMemory)
        {
            builder.Services.AddDbContext<CatalogDbContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Services.Catalogs")
            );

            builder.Services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<CatalogDbContext>()!);
            builder.Services.AddScoped<IDomainEventContext>(provider => provider.GetService<CatalogDbContext>()!);
        }
        else
        {
            builder.Services.AddPostgresDbContext<CatalogDbContext>();
        }

        builder.Services.AddScoped<ICatalogDbContext>(provider => provider.GetRequiredService<CatalogDbContext>());
    }

    private static void AddMongoReadStorage(WebApplicationBuilder builder)
    {
        builder.Services.AddMongoDbContext<CatalogReadDbContext>();
    }
}
