using BuildingBlocks.Logging;
using BuildingBlocks.Security;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Web.Middlewares;
using ECommerce.Services.Identity;
using ECommerce.Services.Identity.Api.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Identity.Api.Extensions.ServiceCollectionExtensions;
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

    var loggingOptions = builder.Configuration.GetSection(nameof(LoggerOptions)).Get<LoggerOptions>();

    builder.AddCompression();

    builder.AddCustomProblemDetails();

    builder.Services.AddSingleton<RevokeAccessTokenMiddleware>();

    builder.Host.AddCustomSerilog(
        optionsBuilder =>
        {
            optionsBuilder.SetLevel(LogEventLevel.Information);
        });

    builder.AddCustomSwagger(builder.Configuration, typeof(IdentityRoot).Assembly);

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

    if (environment.IsDevelopment() || environment.IsEnvironment("docker"))
    {
        app.UseDeveloperExceptionPage();

        // Minimal Api not supported versioning in .net 6
        app.UseCustomSwagger();

        // ref: https://christian-schou.dk/how-to-make-api-documentation-using-swagger/
        app.UseReDoc(options =>
        {
            options.DocumentTitle = "Identity Service ReDoc";
            options.SpecUrl = "/swagger/v1/swagger.json";
        });
    }

    app.UseProblemDetails();
    app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest);
    app.UseRequestLogContextMiddleware();

    app.UseRouting();
    app.UseAppCors();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRevokeAccessTokenMiddleware();

    /*----------------- Module Middleware Setup ------------------*/
    await app.ConfigureModules();

    app.MapControllers();

    /*----------------- Module Routes Setup ------------------*/
    app.MapModulesEndpoints();

    // automatic discover minimal endpoints
    app.MapMinimalEndpoints();

    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();
}

public partial class Program
{
}
