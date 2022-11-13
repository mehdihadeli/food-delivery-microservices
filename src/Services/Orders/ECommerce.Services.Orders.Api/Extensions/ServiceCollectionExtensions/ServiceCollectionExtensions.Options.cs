using BuildingBlocks.Core.Web;

namespace ECommerce.Services.Orders.Api.Extensions.ServiceCollectionExtensions;

internal static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddApplicationOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<AppOptions>().Bind(builder.Configuration.GetSection(nameof(AppOptions)))
            .ValidateDataAnnotations();

        return builder;
    }
}
