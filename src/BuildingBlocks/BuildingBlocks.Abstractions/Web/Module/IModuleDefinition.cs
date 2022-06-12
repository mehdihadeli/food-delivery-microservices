using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Abstractions.Web.Module;

public interface IModuleDefinition
{
    IServiceCollection AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment);

    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
    Task<WebApplication> ConfigureModule(WebApplication app);
}
