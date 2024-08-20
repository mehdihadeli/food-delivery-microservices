using AutoMapper;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Resiliency;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Identity;
using Microsoft.Extensions.Options;

namespace FoodDelivery.Services.Customers.Shared.Clients;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomHttpClients(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedOptions<PolicyOptions>();

        AddCatalogsApiClient(builder);

        AddIdentityApiClient(builder);

        return builder;
    }

    private static void AddIdentityApiClient(WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedOptions<IdentityApiClientOptions>();
        builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(
            (client, sp) =>
            {
                var identityApiOptions = sp.GetRequiredService<IOptions<IdentityApiClientOptions>>();
                var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>();
                identityApiOptions.Value.NotBeNull();
                var mapper = sp.GetRequiredService<IMapper>();

                var baseAddress = identityApiOptions.Value.BaseApiAddress;
                client.BaseAddress = new Uri(baseAddress);
                return new IdentityApiClient(client, mapper, identityApiOptions, policyOptions);
            }
        );
    }

    private static void AddCatalogsApiClient(WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedOptions<CatalogsApiClientOptions>();
        builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>(
            (client, sp) =>
            {
                var catalogApiOptions = sp.GetRequiredService<IOptions<CatalogsApiClientOptions>>();
                var policyOptions = sp.GetRequiredService<IOptions<PolicyOptions>>();
                catalogApiOptions.Value.NotBeNull();
                var mapper = sp.GetRequiredService<IMapper>();

                var baseAddress = catalogApiOptions.Value.BaseApiAddress;
                client.BaseAddress = new Uri(baseAddress);
                return new CatalogApiClient(client, mapper, catalogApiOptions, policyOptions);
            }
        );
    }
}
