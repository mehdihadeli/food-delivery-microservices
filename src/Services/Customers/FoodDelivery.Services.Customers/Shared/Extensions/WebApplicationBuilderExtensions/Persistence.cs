using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Catalogs.Shared.Data;
using FoodDelivery.Services.Customers.Customers.Data.Repositories.Mongo;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;
using FoodDelivery.Services.Customers.RestockSubscriptions.Data.Repositories.Mongo;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodDelivery.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

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
        if (configuration.GetValue<bool>($"{nameof(PostgresOptions)}:{nameof(PostgresOptions.UseInMemory)}"))
        {
            services.AddDbContext<CustomersDbContext>(
                options => options.UseInMemoryDatabase("FoodDelivery.Services.FoodDelivery.Services.Customers")
            );

            services.TryAddScoped<IDbFacadeResolver>(provider => provider.GetService<CustomersDbContext>()!);
            services.TryAddScoped<IDomainEventContext>(provider => provider.GetService<CustomersDbContext>()!);
        }
        else
        {
            services.AddPostgresDbContext<CustomersDbContext>(configuration);

            // add migrations and seeders dependencies, or we could add seeders inner each modules
            services.TryAddScoped<IDataSeeder, CustomersDataSeeder>();
            services.TryAddScoped<IMigrationExecutor, CustomersMigrationExecutor>();
        }

        services.TryAddScoped<ICustomersDbContext>(provider => provider.GetRequiredService<CustomersDbContext>());
    }

    private static void AddMongoReadStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDbContext<CustomersReadDbContext>(configuration);
        services.TryAddTransient<ICustomerReadRepository, CustomerReadRepository>();
        services.TryAddTransient<IRestockSubscriptionReadRepository, RestockSubscriptionReadRepository>();
        services.TryAddTransient<ICustomersReadUnitOfWork, CustomersReadUnitOfWork>();
    }
}
