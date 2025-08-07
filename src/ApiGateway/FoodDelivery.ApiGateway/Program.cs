using BuildingBlocks.Core.Extensions;
using FoodDelivery.ApiGateway;
using FoodDelivery.ServiceDefaults.Extensions;
using Yarp.ReverseProxy.Transforms;
using static BuildingBlocks.Core.Messages.MessageHeaders;

var builder = WebApplication.CreateBuilder(args);

// https://docs.duendesoftware.com/identityserver/v5/bff/apis/remote/
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/transforms
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/getting-started
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/config-files
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("Yarp"))
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

builder.Services.AddCors(options =>
{
    var corsOptions = builder.Configuration.BindOptions<CorsOptions>();
    corsOptions.NotBeNull();
    if (corsOptions.AllowedOrigins.Length == 0)
        throw new InvalidOperationException("At least one origin must be configured in CorsOptions:AllowedOrigins");

    options.AddPolicy(
        "ReactApp",
        policy => policy.WithOrigins(corsOptions.AllowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    );
});

builder.AddServiceDefaults();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapDefaultEndpoints();

// CORS must come before YARP
app.UseCors("ReactApp");

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
