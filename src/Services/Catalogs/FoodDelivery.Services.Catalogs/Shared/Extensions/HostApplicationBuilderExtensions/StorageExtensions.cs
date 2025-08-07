using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo.Extensions;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Shared.Data;
using FoodDelivery.Services.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddStorage(this IHostApplicationBuilder builder)
    {
        AddPostgresWriteStorage(builder);
        AddMongoReadStorage(builder);

        return builder;
    }

    private static void AddPostgresWriteStorage(IHostApplicationBuilder builder)
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
            builder.AddPostgresDbContext<CatalogDbContext>(
                connectionStringName: AspireApplicationResources.PostgresDatabase.Catalogs,
                action: app =>
                {
                    if (app.Environment.IsDevelopment() || app.Environment.IsAspireRun())
                    {
                        // apply migration and seed data for dev environment
                        app.AddMigration<CatalogDbContext, CatalogsDataSeeder>();
                    }
                    else
                    {
                        // just apply migration for production without seeding
                        app.AddMigration<CatalogDbContext>();
                    }
                }
            );
        }

        builder.Services.AddScoped<ICatalogDbContext>(provider => provider.GetRequiredService<CatalogDbContext>());
    }

    private static void AddMongoReadStorage(IHostApplicationBuilder builder)
    {
        builder.AddMongoDbContext<CatalogReadDbContext>(
            connectionStringName: AspireApplicationResources.MongoDatabase.Catalogs
        );
    }
}
