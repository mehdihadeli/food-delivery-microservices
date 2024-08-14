using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Persistence.EfCore.Postgres;
using FoodDelivery.Services.Identity.Shared.Data;
using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodDelivery.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomIdentity(
        this WebApplicationBuilder builder,
        IConfiguration configuration,
        Action<IdentityOptions>? configure = null
    )
    {
        builder.Services.AddValidatedOptions<IdentityOptions>();
        var postgresOptions = builder.Configuration.BindOptions<PostgresOptions>();

        if (postgresOptions.UseInMemory)
        {
            builder.Services.AddDbContext<IdentityContext>(options =>
                options.UseInMemoryDatabase("Shop.Services.FoodDelivery.Services.Identity")
            );

            builder.Services.TryAddScoped<IDbFacadeResolver>(provider => provider.GetService<IdentityContext>()!);
            builder.Services.TryAddScoped<IDomainEventContext>(provider => provider.GetService<IdentityContext>()!);
        }
        else
        {
            // Postgres
            builder.Services.AddPostgresDbContext<IdentityContext>(configuration);

            // add migrations and seeders dependencies, or we could add seeders inner each modules
            builder.Services.TryAddScoped<IDataSeeder, IdentityDataSeeder>();
            builder.Services.TryAddScoped<IMigrationExecutor, IdentityMigrationExecutor>();
        }

        // Problem with .net core identity - will override our default authentication scheme `JwtBearerDefaults.AuthenticationScheme` to unwanted `FoodDelivery.Services.Identity.Application` in `AddIdentity()` method .net identity
        // https://github.com/IdentityServer/IdentityServer4/issues/1525
        // https://github.com/IdentityServer/IdentityServer4/issues/1525
        // some dependencies will add here if not registered before
        builder
            .Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.RequireUniqueEmail = true;

                if (configure is { })
                    configure.Invoke(options);
            })
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        return builder;
    }
}
