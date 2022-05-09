using System.IdentityModel.Tokens.Jwt;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SpectreConsole;
using Yarp.ReverseProxy.Transforms;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.SpectreConsole(
        "{Timestamp:HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception}",
        LogEventLevel.Information)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// https://docs.duendesoftware.com/identityserver/v5/bff/apis/remote/
// https://microsoft.github.io/reverse-proxy/articles/index.html
// https://microsoft.github.io/reverse-proxy/articles/authn-authz.html
// https://microsoft.github.io/reverse-proxy/articles/transforms.html
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("yarp"))
    // .AddTransforms<AccessTokenTransformProvider>()
    .AddTransforms(transforms =>
    {
        // https://microsoft.github.io/reverse-proxy/articles/transforms.html
        transforms.AddRequestTransform(transform =>
        {
            var requestId = Guid.NewGuid().ToString("N");
            var correlationId = Guid.NewGuid().ToString("N");

            transform.ProxyRequest.Headers.Add("X-Request-Id", requestId);
            transform.ProxyRequest.Headers.Add("X-Correlation-Id", correlationId);

            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapGet("/", async (HttpContext context) =>
{
    await context.Response.WriteAsync($"Store Gateway");
});

app.MapReverseProxy();

await app.RunAsync();
