using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Orders.Shared.Contracts;
using FoodDelivery.Services.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodDelivery.Services.Orders.Shared.Extensions.WebApplicationBuilderExtensions;

internal static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddStorage(this WebApplicationBuilder builder)
    {
        AddPostgresWriteStorage(builder.Services, builder.Configuration);
        AddMongoReadStorage(builder.Services, builder.Configuration);

        return builder;
    }

    private static void AddPostgresWriteStorage(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("PostgresOptions.UseInMemory"))
        {
            services.AddDbContext<OrdersDbContext>(
                options => options.UseInMemoryDatabase("FoodDelivery.Services.Orders")
            );

            services.TryAddScoped<IDbFacadeResolver>(provider => provider.GetService<OrdersDbContext>()!);
            services.TryAddScoped<IDomainEventContext>(provider => provider.GetService<OrdersDbContext>()!);
        }
        else
        {
            services.AddPostgresDbContext<OrdersDbContext>(configuration);

            // add migrations and seeders dependencies, or we could add seeders inner each modules
            services.TryAddScoped<IDataSeeder, OrdersDataSeeder>();
            services.TryAddScoped<IMigrationExecutor, OrdersMigrationExecutor>();
        }

        services.TryAddScoped<IOrdersDbContext>(provider => provider.GetRequiredService<OrdersDbContext>());
    }

    private static void AddMongoReadStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDbContext<OrderReadDbContext>(configuration);
    }
}
