using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Resiliency.HttpClient;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Rest;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity.Rest;

namespace FoodDelivery.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomHttpClients(this WebApplicationBuilder builder)
    {
        AddCatalogsApiClient(builder);

        AddIdentityApiClient(builder);

        return builder;
    }

    private static void AddCatalogsApiClient(this WebApplicationBuilder builder)
    {
        AddCatalogsRestClient(builder);
    }

    private static void AddIdentityApiClient(this WebApplicationBuilder builder)
    {
        AddIdentityRestClient(builder);
    }

    private static void AddIdentityRestClient(WebApplicationBuilder builder)
    {
        // rest client
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#typed-clients
        builder.Services.AddValidatedOptions<IdentityRestClientOptions>();
        builder.Services.AddCustomHttpClient<IIdentityRestClient, IdentityRestClient, IdentityRestClientOptions>();
    }

    private static void AddCatalogsRestClient(WebApplicationBuilder builder)
    {
        // rest client
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#typed-clients
        builder.Services.AddValidatedOptions<CatalogsRestClientOptions>();
        builder.Services.AddCustomHttpClient<ICatalogsRestClient, CatalogsRestClient, CatalogsRestClientOptions>();
    }
}
