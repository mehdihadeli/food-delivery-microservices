using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Persistence.EfCore.Postgres;
using FoodDelivery.Services.Identity.Shared.Data;
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
            builder.AddPostgresDbContext<IdentityContext>();
        }

        return builder;
    }
}
