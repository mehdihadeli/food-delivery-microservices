using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Abstractions.Web.MinimalApi;

public interface IMinimalEndpointDefinition
{
    IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder);
}
