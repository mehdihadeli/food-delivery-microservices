using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ECommerce.Services.Orders.Shared.Contracts;
using ECommerce.Services.Orders.Shared.Data;

namespace ECommerce.Services.Orders.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddStorage(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        AddStorage(builder.Services, configuration);

        return builder;
    }

    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        AddPostgresWriteStorage(services, configuration);
        AddMongoReadStorage(services, configuration);

        return services;
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
            services.AddPostgresDbContext<OrdersDbContext>(configuration);
        }

        services.AddScoped<IOrdersDbContext>(provider => provider.GetRequiredService<OrdersDbContext>());
    }

    private static void AddMongoReadStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDbContext<OrderReadDbContext>(configuration);
    }
}
