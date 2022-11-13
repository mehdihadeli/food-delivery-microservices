using BuildingBlocks.Resiliency.Extensions;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;

namespace ECommerce.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomHttpClients(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<IdentityApiClientOptions>()
            .Bind(builder.Configuration.GetSection(nameof(IdentityApiClientOptions))).ValidateDataAnnotations();

        builder.Services.AddOptions<CatalogsApiClientOptions>()
            .Bind(builder.Configuration.GetSection(nameof(CatalogsApiClientOptions)))
            .ValidateDataAnnotations();

        builder.Services.AddHttpApiClient<ICatalogApiClient, CatalogApiClient>();
        builder.Services.AddHttpApiClient<IIdentityApiClient, IdentityApiClient>();

        return builder;
    }
}
