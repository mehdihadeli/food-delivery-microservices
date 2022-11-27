using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Extensions;
using ECommerce.Services.Customers.Api.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Customers.Api.Extensions.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Customers Service").Centered().Color(Color.Blue));

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
// https://benfoster.io/blog/mvc-to-minimal-apis-aspnet-6/
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((env, c) =>
{
    // Handling Captive Dependency Problem
    // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
    // https://levelup.gitconnected.com/top-misconceptions-about-dependency-injection-in-asp-net-core-c6a7afd14eb4
    // https://blog.ploeh.dk/2014/06/02/captive-dependency/
    if (env.HostingEnvironment.IsDevelopment() || env.HostingEnvironment.IsEnvironment("tests") ||
        env.HostingEnvironment.IsStaging())
    {
        c.ValidateScopes = true;
    }
});

builder.Services.AddControllers(options =>
        options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())))
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// https://www.talkingdotnet.com/disable-automatic-model-state-validation-in-asp-net-core-2-1/
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddApplicationOptions(builder.Configuration);

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

// https://learn.microsoft.com/en-us/aspnet/core/diagnostics/asp0014
app.MapControllers();

/*----------------- Module Routes Setup ------------------*/
app.MapModulesEndpoints();

// map registered minimal endpoints
app.MapMinimalEndpoints();

app.MapGet("/test", context => { return Task.FromResult("test");  });

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("docker"))
{
    // swagger middleware should register last to discover all endpoints and its versions correctly
    app.UseCustomSwagger();
}

await app.RunAsync();


namespace ECommerce.Services.Customers.Api
{
    public partial class Program
    {
    }
}
