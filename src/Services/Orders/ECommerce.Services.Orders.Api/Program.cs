using BuildingBlocks.Core;
using BuildingBlocks.Logging;
using BuildingBlocks.Security;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serilog;
using Serilog.Events;
using ECommerce.Services.Orders;
using ECommerce.Services.Orders.Api.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Orders.Api.Extensions.ServiceCollectionExtensions;

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

builder.Services.AddApplicationOptions(builder.Configuration);
var loggingOptions = builder.Configuration.GetSection(nameof(LoggerOptions)).Get<LoggerOptions>();

builder.AddCompression();

builder.AddCustomProblemDetails();

builder.Host.AddCustomSerilog(
    optionsBuilder =>
    {
        optionsBuilder.SetLevel(LogEventLevel.Information);
    },
    config =>
    {
        config.WriteTo.File(
            Program.GetLogPath(builder.Environment, loggingOptions) ?? "../logs/customers-service.log",
            outputTemplate: loggingOptions?.LogTemplate ??
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true);
    });

builder.AddCustomSwagger(builder.Configuration, typeof(OrdersRoot).Assembly);

builder.Services.AddHttpContextAccessor();


builder.Services.AddCustomJwtAuthentication(builder.Configuration);
builder.Services.AddCustomAuthorization(
    rolePolicies: new List<RolePolicy>
    {
        new(OrdersConstants.Role.Admin, new List<string> {OrdersConstants.Role.Admin}),
        new(OrdersConstants.Role.User, new List<string> {OrdersConstants.Role.User})
    });

/*----------------- Module Services Setup ------------------*/
builder.AddModulesServices();

var app = builder.Build();

var environment = app.Environment;

if (environment.IsDevelopment() || environment.IsEnvironment("docker"))
{
    app.UseDeveloperExceptionPage();

    // Minimal Api not supported versioning in .net 6
    app.UseCustomSwagger();

    // ref: https://christian-schou.dk/how-to-make-api-documentation-using-swagger/
    app.UseReDoc(options =>
    {
        options.DocumentTitle = "Customers Service ReDoc";
        options.SpecUrl = "/swagger/v1/swagger.json";
    });
}

ServiceActivator.Configure(app.Services);

app.UseProblemDetails();
app.UseSerilogRequestLogging();

/*----------------- Module Middleware Setup ------------------*/
await app.ConfigureModules();

app.UseRouting();
app.UseAppCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

/*----------------- Module Routes Setup ------------------*/
app.MapModulesEndpoints();

// automatic discover minimal endpoints
app.MapEndpoints();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

await app.RunAsync();


public partial class Program
{
    public static string? GetLogPath(IWebHostEnvironment env, LoggerOptions loggerOptions)
        => env.IsDevelopment() ? loggerOptions.DevelopmentLogPath : loggerOptions.ProductionLogPath;
}
