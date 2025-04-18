using Bogus;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using BuildingBlocks.Web.Extensions.WebApplicationBuilderExtensions;
using BuildingBlocks.Web.Minimal.Extensions;
using FoodDelivery.Services.Catalogs;
using FoodDelivery.Services.Catalogs.Shared;
using FoodDelivery.Services.Catalogs.Shared.Extensions.WebApplicationBuilderExtensions;
using FoodDelivery.Services.Catalogs.Shared.Extensions.WebApplicationExtensions;
using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.Extensions.Primitives;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Catalogs Service").Centered().Color(Color.FromInt32(new Faker().Random.Int(1, 255))));

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
// https://benfoster.io/blog/mvc-to-minimal-apis-aspnet-6/
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(
    (context, options) =>
    {
        var isDevMode =
            context.HostingEnvironment.IsDevelopment()
            || context.HostingEnvironment.IsTest()
            || context.HostingEnvironment.IsStaging();

        // Handling Captive Dependency Problem
        // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
        // https://levelup.gitconnected.com/top-misconceptions-about-dependency-injection-in-asp-net-core-c6a7afd14eb4
        // https://blog.ploeh.dk/2014/06/02/captive-dependency/
        // https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-7.0&viewFallbackFrom=aspnetcore-2.2#scope-validation
        // CreateDefaultBuilder and WebApplicationBuilder in minimal apis sets `ServiceProviderOptions.ValidateScopes` and `ServiceProviderOptions.ValidateOnBuild` to true if the app's environment is Development.
        // check dependencies are used in a valid life-time scope
        options.ValidateScopes = isDevMode;

        // validate dependencies on the startup immediately instead of waiting for using the service - Issue with masstransit #85
        // options.ValidateOnBuild = isDevMode;
    }
);

builder.AddAspnetOpenApi(["v1", "v2"]);

builder.AddInfrastructure();

builder.AddCatalogsServices();

builder.AddCustomVersioning();

// register endpoints
builder.AddMinimalEndpoints(typeof(CatalogsMetadata).Assembly);

var app = builder.Build();

if (app.Environment.IsDependencyTest())
{
    return;
}

app.UseInfrastructure();

app.UseCatalogs();

app.MapCatalogsEndpoints();

// map registered minimal endpoints
app.MapMinimalEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseAspnetOpenApi();
}

await app.RunAsync();
