using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Resiliency.HttpClient;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Rest;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity.Rest;

namespace FoodDelivery.Services.Customers.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomHttpClients(this IHostApplicationBuilder builder)
    {
        AddCatalogsApiClient(builder);

        AddIdentityApiClient(builder);

        return builder;
    }

    private static void AddCatalogsApiClient(this IHostApplicationBuilder builder)
    {
        AddCatalogsRestClient(builder);
    }

    private static void AddIdentityApiClient(this IHostApplicationBuilder builder)
    {
        AddIdentityRestClient(builder);
    }

    private static void AddIdentityRestClient(IHostApplicationBuilder builder)
    {
        // rest client
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#typed-clients
        builder.Services.AddValidationOptions<IdentityRestClientOptions>();
        builder.Services.AddCustomHttpClient<IIdentityRestClient, IdentityRestClient, IdentityRestClientOptions>();
    }

    private static void AddCatalogsRestClient(IHostApplicationBuilder builder)
    {
        // rest client
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#typed-clients
        builder.Services.AddValidationOptions<CatalogsRestClientOptions>();
        builder.Services.AddCustomHttpClient<ICatalogsRestClient, CatalogsRestClient, CatalogsRestClientOptions>();
    }
}
