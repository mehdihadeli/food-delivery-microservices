using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Abstractions.Web.Module;

public interface IModuleConfiguration
{
    WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder);

    Task<WebApplication> ConfigureModule(WebApplication app);

    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}
