using BuildingBlocks.Abstractions.CQRS.Queries;

namespace ECommerce.Services.Identity.Identity.Features.GettingClaims;

public static class GetClaimsEndpoint
{
    internal static IEndpointRouteBuilder MapGetClaimsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{IdentityConfigs.IdentityPrefixUri}/claims", GetClaims)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .Produces<GetClaimsQueryResult>()
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDisplayName("Get User claims");

        return endpoints;
    }

    private static async Task<IResult> GetClaims(
        IQueryProcessor queryProcessor, CancellationToken cancellationToken)
    {
        var result = await queryProcessor.SendAsync(new GetClaimsQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
