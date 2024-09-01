using System.IdentityModel.Tokens.Jwt;
using BuildingBlocks.Logging;
using MassTransit;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Spectre;
using Yarp.ReverseProxy.Transforms;
using MessageHeaders = BuildingBlocks.Core.Messaging.MessageHeaders;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Spectre("{Timestamp:HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception}", LogEventLevel.Information)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// https://docs.duendesoftware.com/identityserver/v5/bff/apis/remote/
// https://microsoft.github.io/reverse-proxy/articles/index.html
// https://microsoft.github.io/reverse-proxy/articles/authn-authz.html
// https://microsoft.github.io/reverse-proxy/articles/transforms.html
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("yarp"))
    // .AddTransforms<AccessTokenTransformProvider>()
    .AddTransforms(transforms =>
    {
        // https://microsoft.github.io/reverse-proxy/articles/transforms.html
        transforms.AddRequestTransform(transform =>
        {
            // add correlation-id in the initial life cycle of the request
            transform.ProxyRequest.Headers.Add(MessageHeaders.CorrelationId, NewId.NextGuid().ToString());

            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

// request logging just log in information level and above as default
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest;
    opts.GetLevel = LogEnricher.GetLogLevel;
});

app.MapGet(
    "/",
    async (HttpContext context) =>
    {
        await context.Response.WriteAsync("FoodDelivery Api Gateway");
    }
);

app.MapReverseProxy();

await app.RunAsync();
