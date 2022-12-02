using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using ECommerce.Services.Identity.Shared.Data;
using ECommerce.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomIdentity(
        this WebApplicationBuilder builder,
        Action<IdentityOptions>? configure = null)
    {
        if (builder.Configuration.GetValue<bool>("PostgresOptions:UseInMemory"))
        {
            builder.Services.AddDbContext<IdentityContext>(options =>
                options.UseInMemoryDatabase("Shop.Services.ECommerce.Services.Identity"));

            builder.Services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<IdentityContext>()!);
        }
        else
        {
            // Postgres
            builder.Services.AddPostgresDbContext<IdentityContext>();
        }

        // Problem with .net core identity - will override our default authentication scheme `JwtBearerDefaults.AuthenticationScheme` to unwanted `ECommerce.Services.Identity.Application` in `AddIdentity()` method .net identity
        // https://github.com/IdentityServer/IdentityServer4/issues/1525
        // https://github.com/IdentityServer/IdentityServer4/issues/1525
        // some dependencies will add here if not registered before
        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
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

        if (builder.Configuration.GetSection(nameof(IdentityOptions)) is not null)
            builder.Services.Configure<IdentityOptions>(builder.Configuration.GetSection(nameof(IdentityOptions)));

        return builder;
    }
}
