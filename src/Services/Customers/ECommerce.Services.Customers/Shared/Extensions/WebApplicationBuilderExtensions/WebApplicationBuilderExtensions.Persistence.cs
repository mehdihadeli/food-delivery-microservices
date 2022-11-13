using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Customers.Shared.Contracts;
using ECommerce.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddStorage(this WebApplicationBuilder builder)
    {
        AddPostgresWriteStorage(builder.Services, builder.Configuration);
        AddMongoReadStorage(builder.Services, builder.Configuration);

        return builder;
    }

    private static void AddPostgresWriteStorage(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("PostgresOptions:UseInMemory"))
        {
            services.AddDbContext<CustomersDbContext>(options =>
                options.UseInMemoryDatabase("ECommerce.Services.ECommerce.Services.Customers"));

            services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<CustomersDbContext>()!);
        }
        else
        {
            services.AddPostgresDbContext<CustomersDbContext>(configuration);
        }

        services.AddScoped<ICustomersDbContext>(provider => provider.GetRequiredService<CustomersDbContext>());
    }

    private static void AddMongoReadStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDbContext<CustomersReadDbContext>(configuration);
    }
}
