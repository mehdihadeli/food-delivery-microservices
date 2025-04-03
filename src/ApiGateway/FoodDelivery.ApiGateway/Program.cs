using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web.HeaderPropagation.Extensions;
using BuildingBlocks.Observability.Extensions;
using MassTransit;
using Yarp.ReverseProxy.Transforms;
using static BuildingBlocks.Core.Messages.MessageHeaders;

var builder = WebApplication.CreateBuilder(args);

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
            transform.ProxyRequest.Headers.Add(CorrelationId, NewId.NextGuid().ToString());

            return ValueTask.CompletedTask;
        });
    });

builder.AddCore();
builder.AddCustomObservability();

builder.Services.AddHeaderPropagation(options =>
{
    options.Headers.Add(CorrelationId);
    options.Headers.Add(CausationId);
});

var app = builder.Build();

app.UseHeaderPropagation();

app.MapGet(
    "/",
    async (HttpContext context) =>
    {
        await context.Response.WriteAsync("Api Gateway.");
    }
);

app.MapReverseProxy();

await app.RunAsync();
