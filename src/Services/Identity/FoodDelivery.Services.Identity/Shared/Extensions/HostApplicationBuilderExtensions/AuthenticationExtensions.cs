using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FoodDelivery.Services.Identity.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthentication(this IHostApplicationBuilder builder)
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security
        var oauthOptions = builder.Configuration.BindOptions<OAuthOptions>();
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = oauthOptions.Authority;
                options.Audience = oauthOptions.Audience;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = oauthOptions.ValidateIssuer,
                    ValidIssuers = oauthOptions.ValidIssuers,
                    ValidateAudience = oauthOptions.ValidateAudience,
                    ValidAudiences = oauthOptions.ValidAudiences,
                    ValidateLifetime = oauthOptions.ValidateLifetime,
                    ClockSkew = oauthOptions.ClockSkew,
                    // For IdentityServer4/Duende, we should also validate the signing key
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "name", // Map "name" claim to User.Identity.Name
                    RoleClaimType = "role", // Map "role" claim to User.IsInRole()
                };

                // Preserve ALL claims from the token (including "sub")
                options.MapInboundClaims = false;
            });

        return builder;
    }
}
