using BuildingBlocks.Logging;
using BuildingBlocks.Security;
using BuildingBlocks.Security.Extensions;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Web.Middlewares;
using ECommerce.Services.Identity;
using ECommerce.Services.Identity.Api.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Identity.Api.Extensions.WebApplicationBuilderExtensions;
using ECommerce.Services.Identity.Api.Middlewares;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serilog;
using Serilog.Events;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Identity Service").Centered().Color(Color.Pink1));

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
// https://benfoster.io/blog/mvc-to-minimal-apis-aspnet-6/
var builder = WebApplication.CreateBuilder(args);

RegisterServices(builder);

var app = builder.Build();

await ConfigureApplication(app);

await app.RunAsync();

static void RegisterServices(WebApplicationBuilder builder)
{
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

    // https://www.michaco.net/blog/EnvironmentVariablesAndConfigurationInASPNETCoreApps#environment-variables-and-configuration
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#non-prefixed-environment-variables
    builder.Configuration.AddEnvironmentVariables("ecommerce_identity_env_");

    // https://github.com/tonerdo/dotnet-env
    DotNetEnv.Env.TraversePath().Load();

    builder.Services.AddControllers(options =>
            options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())))
        .AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

    // https://www.talkingdotnet.com/disable-automatic-model-state-validation-in-asp-net-core-2-1/
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.AddCompression();

    builder.AddCustomProblemDetails();

    builder.Services.AddSingleton<RevokeAccessTokenMiddleware>();

    builder.AddCustomSerilog(
        optionsBuilder =>
        {
            optionsBuilder.SetLevel(LogEventLevel.Information);
        });

    builder.Services.AddCustomVersioning();
    builder.AddCustomSwagger(typeof(IdentityRoot).Assembly);

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddCustomJwtAuthentication(builder.Configuration);

    builder.Services.AddCustomAuthorization(
        rolePolicies: new List<RolePolicy>
        {
            new(IdentityConstants.Role.Admin, new List<string> {IdentityConstants.Role.Admin}),
            new(IdentityConstants.Role.User, new List<string> {IdentityConstants.Role.User})
        });

    /*----------------- Module Services Setup ------------------*/
    builder.AddModulesServices();
}

static async Task ConfigureApplication(WebApplication app)
{
    var environment = app.Environment;

    app.UseProblemDetails();

    // https://thecodeblogger.com/2021/05/27/asp-net-core-web-application-routing-and-endpoint-internals/
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-7.0#routing-basics
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-7.0#endpoints
    // https://stackoverflow.com/questions/57846127/what-are-the-differences-between-app-userouting-and-app-useendpoints
    // in .net 6 and above we don't need UseRouting and UseEndpoints but if ordering is important we should write it
    // app.UseRouting();


    app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest);
    app.UseRequestLogContextMiddleware();

    app.UseAppCors();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRevokeAccessTokenMiddleware();

    /*----------------- Module Middleware Setup ------------------*/
    await app.ConfigureModules();

    // https://learn.microsoft.com/en-us/aspnet/core/diagnostics/asp0014
    app.MapControllers();

    /*----------------- Module Routes Setup ------------------*/
    app.MapModulesEndpoints();

    // automatic discover minimal endpoints
    //app.MapMinimalEndpoints();

    if (environment.IsDevelopment() || environment.IsEnvironment("docker"))
    {
        // swagger middleware should register last to discover all endpoints and its versions correctly
        app.UseCustomSwagger();
    }

    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();
}

public partial class Program
{
}
