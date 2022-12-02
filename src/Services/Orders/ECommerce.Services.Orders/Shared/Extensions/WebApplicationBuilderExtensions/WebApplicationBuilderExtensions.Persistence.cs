using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Orders.Shared.Contracts;
using ECommerce.Services.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Orders.Shared.Extensions.WebApplicationBuilderExtensions;

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
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase("ECommerce.Services.Customers"));

            services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<OrdersDbContext>()!);
        }
        else
        {
            services.AddPostgresDbContext<OrdersDbContext>();
        }

        services.AddScoped<IOrdersDbContext>(provider => provider.GetRequiredService<OrdersDbContext>());
    }

    private static void AddMongoReadStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDbContext<OrderReadDbContext>();
    }
}
