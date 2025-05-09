using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Security;
using BuildingBlocks.Observability.Extensions;
using BuildingBlocks.Security.Jwt;
using FoodDelivery.Services.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;
using static BuildingBlocks.Core.Messages.MessageHeaders;

var builder = WebApplication.CreateBuilder(args);

// https://docs.duendesoftware.com/identityserver/v5/bff/apis/remote/
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/transforms
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/getting-started
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/config-files
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("yarp"))
    .AddTransforms(transforms =>
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/transforms
        transforms.AddRequestTransform(context =>
        {
            // add correlation-id in the initial life cycle of the request
            context.ProxyRequest.Headers.Add(CorrelationId, Guid.NewGuid().ToString());

            var request = context.HttpContext.Request;

            // Forward Authorization if present
            if (
                request.Headers.TryGetValue("Authorization", out var authHeader)
                && !context.ProxyRequest.Headers.Contains("Authorization")
            )
            {
                context.ProxyRequest.Headers.Add("Authorization", authHeader.ToString());
            }

            // Content-Type belongs to the HTTP body, not the general headers. To set it correctly in YARP, use ProxyRequest.Content.Headers.ContentType, not ProxyRequest.Headers
            if (request.ContentType != null && context.ProxyRequest.Content is { Headers.ContentType: null })
            {
                context.ProxyRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    request.ContentType
                );
            }

            var userId = context.HttpContext.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && !context.ProxyRequest.Headers.Contains("X-User-Id"))
            {
                context.ProxyRequest.Headers.Add("X-User-Id", userId);
            }

            // - When `RequestHeadersCopy: false`, the original request headers are not automatically copied to context.ProxyRequest.Headers. So by default, headers like Cookie, X-Forwarded-For, etc., won’t be passed to the downstream service.
            // - Adding explicit RequestHeaderRemove and ResponseHeaderRemove entries is a best practice that prevents unintended headers, Middleware, third-party components, or future code could add those headers back later in the pipeline after our transform runs.

            return ValueTask.CompletedTask;
        });
    });

builder.AddCore();
builder.AddCustomObservability();

builder.Services.AddHeaderPropagation(options =>
{
    options.Headers.Add(CorrelationId);
    options.Headers.Add(CausationId);
});

var jwtOptions = builder.Configuration.BindOptions<JwtOptions>();
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = jwtOptions.Authority;
        options.Audience = jwtOptions.Audience;
        options.RequireHttpsMetadata = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = jwtOptions.ValidateIssuer,
            ValidIssuers = jwtOptions.ValidIssuers,
            ValidateAudience = jwtOptions.ValidateAudience,
            ValidAudiences = jwtOptions.ValidAudiences,
            ValidateLifetime = jwtOptions.ValidateLifetime,
            ClockSkew = jwtOptions.ClockSkew,
            // For IdentityServer4/Duende, we should also validate the signing key
            ValidateIssuerSigningKey = true,
            NameClaimType = "name", // Map "name" claim to User.Identity.Name
            RoleClaimType = "role", // Map "role" claim to User.IsInRole()
        };

        // Preserve ALL claims from the token (including "sub")
        options.MapInboundClaims = false;
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "GatewayAccess",
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(ClaimsType.Scope, Scopes.Gateway);
            policy.RequireClaim(ClaimsType.Permission, Permissions.GatewayAccess);
        }
    );
});

var app = builder.Build();

app.UseHeaderPropagation();

app.MapGet(
    "/",
    async (HttpContext context) =>
    {
        await context.Response.WriteAsync("Api Gateway.");
    }
);

app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

await app.RunAsync();
