using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Customers.Shared;

namespace ECommerce.Services.Customers.RestockSubscriptions;

public class RestockSubscriptionsConfigs : IModuleConfiguration
{
    public const string Tag = "RestockSubscriptions";

    public const string RestockSubscriptionsUrl =
        $"{SharedModulesConfiguration.CustomerModulePrefixUri}/restock-subscriptions";

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        //// we could add event mappers manually, also they can find automatically by scanning assemblies
        // builder.Services.AddSingleton<IEventMapper, RestockSubscriptionsEventMapper>();

        return builder;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult<WebApplication>(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Here we can add endpoints manually but, if our endpoint inherits from `IMinimalEndpointDefinition`, they discover automatically.
        return endpoints;
    }
}
