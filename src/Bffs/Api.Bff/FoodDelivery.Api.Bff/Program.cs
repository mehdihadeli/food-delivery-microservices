using System.Net;
using System.Net.Http.Headers;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.ServiceDefaults.Extensions;
using FoodDelivery.WebApp.Bff;
using FoodDelivery.WebApp.Bff.Clients;
using FoodDelivery.WebApp.Bff.Contracts;
using FoodDelivery.WebApp.Bff.Extensions;
using FoodDelivery.WebApp.Bff.Extensions.HostApplicationBuilderExtensions;
using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("Yarp"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            // Forward Authorization if present for yarp configured endpoints, for local bff endpoints we should forward Authorization header using HttpClientAuthorizationDelegatingHandler
            if (
                transformContext.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader)
                && !transformContext.ProxyRequest.Headers.Contains("Authorization")
            )
            {
                transformContext.ProxyRequest.Headers.Add("Authorization", authHeader.ToString());
            }

            // Content-Type belongs to the HTTP body, not the general headers. To set it correctly in YARP, use ProxyRequest.Content.Headers.ContentType, not ProxyRequest.Headers
            if (
                transformContext.HttpContext.Request.ContentType != null
                && transformContext.ProxyRequest.Content is { Headers.ContentType: null }
            )
            {
                transformContext.ProxyRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(
                    transformContext.HttpContext.Request.ContentType
                );
            }
        });
    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;

    // Clear default networks/proxies (optional, but recommended for strict control)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    // Add YARP's localhost IP as a trusted proxy
    options.KnownProxies.Add(IPAddress.Parse("::1"));
});

builder.AddCustomAuthentication();
builder.AddCustomAuthorization();

// use for local endpoints, external endpoints use yarp and its transformer to add Authorization header
builder.Services.AddTransient<HttpClientAuthorizationDelegatingHandler>();

// - For BFF aggregate endpoints and local endpoints in bff, avoid using YARP and instead call microservices directly with typed HttpClient instances. YARP adds unnecessary latency (BFF → YARP → Microservice) and overhead, whereas direct calls (BFF → Microservice)
// are faster and allow better control over retries, timeouts, and caching. Use YARP only for passthrough routes, not aggregation (requests from outside the BFF), not for internal BFF logic like aggregates.
// - Client with user interaction and it is not machine-to-machine communication (client credential)
builder
    .Services.AddHttpClient<ICatalogsClient, CatalogsClient>(
        (provider, client) =>
        {
            var catalogsAddress = builder.Configuration["Clients:CatalogsClient"].NotBeEmptyOrNull();

            client.BaseAddress = new Uri(catalogsAddress);
        }
    )
    .AddHttpClientAuthorization();

builder
    .Services.AddHttpClient<ICustomersClient, CustomersClient>(
        (provider, client) =>
        {
            var customersAddress = builder.Configuration["Clients:CustomersClient"].NotBeEmptyOrNull();

            client.BaseAddress = new Uri(customersAddress);
        }
    )
    .AddHttpClientAuthorization();

var app = builder.Build();

// - Forwarded Headers Middleware should run before other middleware.
// - `X-Forwarded` is enabled by default to `Set` on yarp transformers. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/transforms-request?view=aspnetcore-9.0#x-forwarded
// - Reads standard forwarded headers (X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host) and updates the request information accordingly,
// Ensures the application sees the original client IP, protocol (HTTP/HTTPS), and host rather than the proxy's information and set them on Context.Request, but we can access to original values through Request.Headers and `X-Original-Host`, `X-Original-For`
app.UseForwardedHeaders();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.MapGet("/", (HttpContext context) => "Web Bff.").AllowAnonymous();

app.MapGet("/api", (HttpContext context) => "Protected endpoint").RequireAuthorization();

// bff local endpoints, external endpoints handle by Yarp
app.MapAllLocalEndpoints();

app.Run();
