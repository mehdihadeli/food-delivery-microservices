using Bogus;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Web;
using BuildingBlocks.Security.Extensions;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Extensions;
using ECommerce.Services.Identity;
using ECommerce.Services.Identity.Api.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Identity.Api.Middlewares;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Identity Service").Centered().Color(Color.FromInt32(new Faker().Random.Int(1, 255))));

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
// https://benfoster.io/blog/mvc-to-minimal-apis-aspnet-6/
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    // Handling Captive Dependency Problem
    // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
    // https://levelup.gitconnected.com/top-misconceptions-about-dependency-injection-in-asp-net-core-c6a7afd14eb4
    // https://blog.ploeh.dk/2014/06/02/captive-dependency/
    // https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
    options.ValidateScopes = context.HostingEnvironment.IsDevelopment() || context.HostingEnvironment.IsTest() ||
                             context.HostingEnvironment.IsStaging();

    // Issue with masstransit #85
    // options.ValidateOnBuild = true;
});

builder.Services.AddControllers(options =>
        options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())))
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        // https://stackoverflow.com/questions/36452468/swagger-ui-web-api-documentation-present-enums-as-strings
        // options.SerializerSettings.Converters.Add(new StringEnumConverter()); // sending enum string to and from client instead of number
    })
    .AddControllersAsServices();

// https://www.talkingdotnet.com/disable-automatic-model-state-validation-in-asp-net-core-2-1/
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddSingleton<RevokeAccessTokenMiddleware>();

builder.Services.AddValidatedOptions<AppOptions>();

// register endpoints
builder.AddMinimalEndpoints();

/*----------------- Module Services Setup ------------------*/
builder.AddModulesServices();

var app = builder.Build();

/*----------------- Module Middleware Setup ------------------*/
await app.ConfigureModules();

// https://thecodeblogger.com/2021/05/27/asp-net-core-web-application-routing-and-endpoint-internals/
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-7.0#routing-basics
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-7.0#endpoints
// https://stackoverflow.com/questions/57846127/what-are-the-differences-between-app-userouting-and-app-useendpoints
// in .net 6 and above we don't need UseRouting and UseEndpoints but if ordering is important we should write it
// app.UseRouting();
app.UseAppCors();

app.UseRevokeAccessTokenMiddleware();

// https://learn.microsoft.com/en-us/aspnet/core/diagnostics/asp0014
app.MapControllers();

/*----------------- Module Routes Setup ------------------*/
app.MapModulesEndpoints();

// automatic discover minimal endpoints
app.MapMinimalEndpoints();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("docker"))
{
    // swagger middleware should register last to discover all endpoints and its versions correctly
    app.UseCustomSwagger();
}

await app.RunAsync();

public partial class Program
{
}
