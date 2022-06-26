using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Abstractions.Web.Module;

public interface IModuleConfiguration
{
    IServiceCollection AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment);

    Task<WebApplication> ConfigureModule(WebApplication app);

    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}
