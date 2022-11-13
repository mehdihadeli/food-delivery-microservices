using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Abstractions.Web.Module;

public interface ISharedModulesConfiguration
{
    WebApplicationBuilder AddSharedModuleServices(WebApplicationBuilder builder);

    Task<WebApplication> ConfigureSharedModule(WebApplication app);

    IEndpointRouteBuilder MapSharedModuleEndpoints(IEndpointRouteBuilder endpoints);
}
