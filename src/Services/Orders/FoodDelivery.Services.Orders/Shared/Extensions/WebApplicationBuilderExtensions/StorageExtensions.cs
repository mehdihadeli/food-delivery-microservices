using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo.Extensions;
using FoodDelivery.Services.Orders.Shared.Contracts;
using FoodDelivery.Services.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Orders.Shared.Extensions.WebApplicationBuilderExtensions;

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
            builder.Services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Services.Orders")
            );

            builder.Services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<OrdersDbContext>()!);
            builder.Services.AddScoped<IDomainEventContext>(provider => provider.GetService<OrdersDbContext>()!);
        }
        else
        {
            builder.Services.AddPostgresDbContext<OrdersDbContext>();
        }

        builder.Services.AddScoped<IOrdersDbContext>(provider => provider.GetRequiredService<OrdersDbContext>());
    }

    private static void AddMongoReadStorage(WebApplicationBuilder builder)
    {
        builder.Services.AddMongoDbContext<OrderReadDbContext>();
    }
}
