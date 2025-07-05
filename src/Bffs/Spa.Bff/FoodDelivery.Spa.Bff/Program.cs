using System.Net.Http.Headers;
using BuildingBlocks.Core.Diagnostics.Extensions;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.OpenTelemetry.Extensions;
using Duende.Bff.Yarp;
using FoodDelivery.Services.Shared.Extensions;
using FoodDelivery.Spa.Bff.Clients;
using FoodDelivery.Spa.Bff.Contracts;
using FoodDelivery.Spa.Bff.Extensions;
using FoodDelivery.Spa.Bff.Extensions.HostApplicationBuilderExtensions;
using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder
    .Services.AddBff()
    // https://docs.duendesoftware.com/bff/fundamentals/apis/remote/
    .AddRemoteApis();

// https://docs.duendesoftware.com/bff/fundamentals/apis/yarp/
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("Yarp"))
    .AddBffExtensions()
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            // Forward Authorization if present for yarp configured endpoints, for local bff endpoints we should forward Authorization header using AddUserAccessTokenHandler

            // // https://docs.duendesoftware.com/accesstokenmanagement/web-apps/
            // // we can add token to external endpoints using `Duende.Bff.Yarp.TokenType` metadata in config or propagate token manually
            // var accessToken = await transformContext.HttpContext.GetUserAccessTokenAsync();
            // if (accessToken.AccessToken is not null)
            // {
            //     transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue(
            //         JwtBearerDefaults.AuthenticationScheme,
            //         accessToken.AccessToken
            //     );
            // }

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
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// - For BFF aggregate endpoints and local endpoints in bff, avoid using YARP and instead call microservices directly with typed HttpClient instances. YARP adds unnecessary latency (BFF → YARP → Microservice) and overhead, whereas direct calls (BFF → Microservice)
// are faster and allow better control over retries, timeouts, and caching. Use YARP only for passthrough routes, not aggregation (requests from outside the BFF), not for internal BFF logic like aggregates.
// - Client with user interaction and it is not machine-to-machine communication (client credential)
// https://docs.duendesoftware.com/accesstokenmanagement/web-apps/
builder
    .Services.AddHttpClient<ICatalogsClient, CatalogsClient>(
        (provider, client) =>
        {
            var catalogsAddress = builder.Configuration["Clients:CatalogsClient"].NotBeEmptyOrNull();
            client.BaseAddress = new Uri(catalogsAddress);
        }
    )
    .AddUserAccessTokenHandler();

builder
    .Services.AddHttpClient<ICustomersClient, CustomersClient>(
        (provider, client) =>
        {
            var customersAddress = builder.Configuration["Clients:CustomersClient"].NotBeEmptyOrNull();
            client.BaseAddress = new Uri(customersAddress);
        }
    )
    .AddUserAccessTokenHandler();

builder.AddCustomAuthentication();
builder.AddCustomAuthorization();

var app = builder.Build();

// Reads standard forwarded headers (X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host) and updates the request information accordingly,
// Ensures the application sees the original client IP, protocol (HTTP/HTTPS), and host rather than the proxy's information
app.UseForwardedHeaders();

app.MapDefaultEndpoints();

app.UseAuthentication();

// https://docs.duendesoftware.com/bff/fundamentals/apis/local/#setup
// Enforces Anti-Forgery (CSRF) Checks for BFF Endpoints, with decorating endpoints for pre/post processing using `AsBffApiEndpoint`
// Blocks Direct AJAX Calls to UI Endpoints - `/bff/login` or `/bff/logout` can't be called via ajax, it should be full-page redirects
// Add the BFF middleware which performs anti forgery protection
app.UseBff();
app.UseAuthorization();

// Add the BFF management endpoints, such as login, logout, etc.
// This has to be added after 'UseAuthorization()'
app.MapBffManagementEndpoints();

app.Map(
    "/",
    context =>
    {
        var reactSpa = builder.Configuration["Redirects:ReactSpaAddress"];
        ArgumentException.ThrowIfNullOrEmpty(reactSpa);
        context.Response.Redirect(reactSpa, permanent: true); // 302
        return Task.CompletedTask;
    }
);

app.MapGet("/api", (HttpContext context) => "Protected endpoint").RequireAuthorization();

// https://docs.duendesoftware.com/bff/fundamentals/apis/yarp/#anti-forgery-protection
app.MapReverseProxy(proxyApp =>
    {
        proxyApp.UseAntiforgeryCheck();
    })
    // https://docs.duendesoftware.com/bff/fundamentals/apis/yarp/#anti-forgery-protection
    .AsBffApiEndpoint();

// bff local endpoints, external endpoints handle by Yarp
app.MapAllLocalEndpoints();

app.Run();
