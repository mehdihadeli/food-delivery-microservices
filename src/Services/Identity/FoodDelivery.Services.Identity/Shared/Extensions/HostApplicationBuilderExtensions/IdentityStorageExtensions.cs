using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using FoodDelivery.Services.Identity.Shared.Data;
using FoodDelivery.Services.Shared.Constants;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddIdentityStorage(this IHostApplicationBuilder builder)
    {
        var option = builder.Configuration.BindOptions<PostgresOptions>();
        if (option.UseInMemory)
        {
            builder.Services.AddDbContext<IdentityContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Services.Identity")
            );

            builder.Services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<IdentityContext>()!);
            builder.Services.AddScoped<IDomainEventContext>(provider => provider.GetService<IdentityContext>()!);
        }
        else
        {
            builder.AddPostgresDbContext<IdentityContext>(
                connectionStringName: AspireApplicationResources.PostgresDatabase.Identity,
                action: app =>
                {
                    if (app.Environment.IsDevelopment() || app.Environment.IsAspireRun())
                    {
                        // apply migration and seed data for dev environment
                        app.AddMigration<IdentityContext, IdentityDataSeeder>();
                    }
                    else
                    {
                        // just apply migration for production without seeding
                        app.AddMigration<IdentityContext>();
                    }
                }
            );
        }

        return builder;
    }
}
