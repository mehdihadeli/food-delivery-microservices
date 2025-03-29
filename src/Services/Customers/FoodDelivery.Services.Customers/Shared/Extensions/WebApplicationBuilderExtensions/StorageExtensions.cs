using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo.Extensions;
using FoodDelivery.Services.Customers.Customers.Data.Repositories.Mongo;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;
using FoodDelivery.Services.Customers.RestockSubscriptions.Data.Repositories.Mongo;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

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
            builder.Services.AddDbContext<CustomersDbContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Services.Customers")
            );

            builder.Services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<CustomersDbContext>()!);
            builder.Services.AddScoped<IDomainEventContext>(provider => provider.GetService<CustomersDbContext>()!);
        }
        else
        {
            builder.Services.AddPostgresDbContext<CustomersDbContext>();
        }

        builder.Services.AddScoped<ICustomersDbContext>(provider => provider.GetRequiredService<CustomersDbContext>());
    }

    private static void AddMongoReadStorage(WebApplicationBuilder builder)
    {
        builder.Services.AddMongoDbContext<CustomersReadDbContext>();
        builder.Services.AddTransient<ICustomerReadRepository, CustomerReadRepository>();
        builder.Services.AddTransient<IRestockSubscriptionReadRepository, RestockSubscriptionReadRepository>();
        builder.Services.AddTransient<ICustomersReadUnitOfWork, CustomersReadUnitOfWork>();
    }
}
