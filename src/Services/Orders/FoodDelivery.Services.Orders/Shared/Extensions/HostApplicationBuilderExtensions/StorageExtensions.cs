using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo.Extensions;
using FoodDelivery.Services.Orders.Shared.Contracts;
using FoodDelivery.Services.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Orders.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
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
            builder.Services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Services.Orders")
            );

            builder.Services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<OrdersDbContext>()!);
            builder.Services.AddScoped<IDomainEventContext>(provider => provider.GetService<OrdersDbContext>()!);
        }
        else
        {
            builder.AddPostgresDbContext<OrdersDbContext>();
        }

        builder.Services.AddScoped<IOrdersDbContext>(provider => provider.GetRequiredService<OrdersDbContext>());
    }

    private static void AddMongoReadStorage(IHostApplicationBuilder builder)
    {
        builder.AddMongoDbContext<OrderReadDbContext>();
    }
}
