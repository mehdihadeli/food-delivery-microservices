using BuildingBlocks.Core.Extensions;
using FoodDelivery.BlazorWebApp;
using FoodDelivery.BlazorWebApp.Components;
using FoodDelivery.BlazorWebApp.Contracts;
using FoodDelivery.BlazorWebApp.Extensions;
using FoodDelivery.BlazorWebApp.Extensions.WebApplicationBuilderExtensions;
using FoodDelivery.BlazorWebApp.Services;
using FoodDelivery.ServiceDefaults.Extensions;

// blazor server-side rendering
var builder = WebApplication.CreateBuilder(args);

// register the services needed to render server-side components in the application
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.AddServiceDefaults();

builder.Services.AddTransient<HttpClientAuthorizationDelegatingHandler>();

builder.Services.AddSingleton<IAccountsService, AccountsService>();
builder.Services.AddSingleton<ICatalogsServiceClient, CatalogsServiceClient>();
builder.Services.AddSingleton<ICustomersServiceClient, CustomersServiceClient>();

builder
    .Services.AddHttpClient(
        "ApiGatewayClient",
        configureClient: client =>
        {
            var gatewayWebAppBff = builder.Configuration.GetValue<string>("ApiGatewayAddress");
            ArgumentException.ThrowIfNullOrEmpty(gatewayWebAppBff);
            client.BaseAddress = new Uri(gatewayWebAppBff);
        }
    )
    .AddHttpClientAuthorization();

builder.AddCustomAuthentication();
builder.AddCustomAuthorization();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

// A middleware to take a root component that’s used to identify the assembly to scan for routable components
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
