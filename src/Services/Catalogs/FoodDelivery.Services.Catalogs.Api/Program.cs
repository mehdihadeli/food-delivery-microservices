using Bogus;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Web;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Minimal.Extensions;
using BuildingBlocks.Web.Modules;
using BuildingBlocks.Web.Modules.Extensions;
using FoodDelivery.Services.Catalogs;
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
        // check dependencies are used in a valid life time scope
        options.ValidateScopes = isDevMode;

        // validate dependencies on the startup immediately instead of waiting for using the service - Issue with masstransit #85
        // options.ValidateOnBuild = isDevMode;
    }
);

// https://www.talkingdotnet.com/disable-automatic-model-state-validation-in-asp-net-core-2-1/
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddValidatedOptions<AppOptions>();

// register endpoints
builder.AddMinimalEndpoints(typeof(CatalogsMetadata).Assembly);

/*----------------- Module Services Setup ------------------*/
builder.AddModulesServices();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsTest())
{
    app.Services.ValidateDependencies(
        builder.Services,
        typeof(CatalogsMetadata).Assembly,
        Assembly.GetExecutingAssembly()
    );
}

/*----------------- Module Middleware Setup ------------------*/
await app.ConfigureModules();

// https://thecodeblogger.com/2021/05/27/asp-net-core-web-application-routing-and-endpoint-internals/
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-7.0#routing-basics
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-7.0#endpoints
// https://stackoverflow.com/questions/57846127/what-are-the-differences-between-app-userouting-and-app-useendpoints
// in .net 6 and above we don't need UseRouting and UseEndpoints but if ordering is important we should write it
// app.UseRouting();

/*----------------- Module Routes Setup ------------------*/
app.MapModulesEndpoints();

// automatic discover minimal endpoints
app.MapMinimalEndpoints();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("docker"))
{
    // should register as last middleware for discovering all endpoints and its versions correctly
    app.UseCustomSwagger();
}

await app.RunAsync();
