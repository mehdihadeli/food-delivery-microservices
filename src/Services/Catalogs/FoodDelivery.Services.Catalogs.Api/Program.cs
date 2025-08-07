using System.Net;
using Bogus;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using BuildingBlocks.OpenApi.AsyncApi;
using FoodDelivery.ServiceDefaults.Extensions;
using FoodDelivery.Services.Catalogs;
using FoodDelivery.Services.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;
using FoodDelivery.Services.Catalogs.Shared.Extensions.WebApplicationExtensions;
using Microsoft.AspNetCore.HttpOverrides;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Catalogs Service").Centered().Color(Color.FromInt32(new Faker().Random.Int(1, 255))));

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddInfrastructure();

builder.AddApplicationServices();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // set `XForwardedPrefix` when we are ensured that we want to set our `BasePath` based on passed `X-Forwarded-Prefix` in the header
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;

    // Clear default networks/proxies (optional, but recommended for strict control)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    // Add YARP's localhost IP as a trusted proxy
    options.KnownProxies.Add(IPAddress.Parse("::1"));
});

var app = builder.Build();

if (app.Environment.IsDependencyTest())
    return;

app.Use(
    async (context, next) =>
    {
        Console.WriteLine("Inline middleware before");
        await next.Invoke();
        Console.WriteLine("Inline middleware after");
    }
);

// https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer
// https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-9.0&tabs=linux-ubuntu
// - X-Forwarded-Prefix header that set by yarp proxy use for setting `Request.BasePath` to `/auth` that use for URL generation or redirection
// - `X-Forwarded` is enabled by default to `Set` on yarp transformers. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/transforms-request?view=aspnetcore-9.0#x-forwarded
// - Reads standard forwarded headers (X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host, X-Forwarded-Prefix) and updates the request information accordingly,
// Ensures the application sees the original client IP, protocol (HTTP/HTTPS), and host rather than the proxy's information and set them on Context.Request, but we can access to original values through Request.Headers and `X-Original-Host`, `X-Original-For`
app.UseForwardedHeaders();

app.MapDefaultEndpoints();

app.UseInfrastructure();

app.MapApplicationEndpoints();

app.UseAspnetOpenApi();
app.UseAsyncApi();

await app.RunAsync();
