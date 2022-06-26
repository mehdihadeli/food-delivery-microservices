using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Abstractions.Web.MinimalApi;

public interface IMinimalEndpointConfiguration
{
    IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder);
}
