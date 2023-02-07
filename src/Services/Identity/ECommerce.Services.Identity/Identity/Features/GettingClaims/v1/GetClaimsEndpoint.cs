using BuildingBlocks.Abstractions.CQRS.Queries;
using Hellang.Middleware.ProblemDetails;

namespace ECommerce.Services.Identity.Identity.Features.GettingClaims.v1;

public static class GetClaimsEndpoint
{
    internal static RouteHandlerBuilder MapGetClaimsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/claims", GetClaims)
            .RequireAuthorization()
            .Produces<GetClaimsResponse>()
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status401Unauthorized)
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Getting User Claims", Description = "Getting User Claims"
            })
            .WithDisplayName("Get User claims");
    }

    private static async Task<IResult> GetClaims(
        IQueryProcessor queryProcessor, CancellationToken cancellationToken)
    {
        var result = await queryProcessor.SendAsync(new GetClaims(), cancellationToken);

        return Results.Ok(result);
    }
}
