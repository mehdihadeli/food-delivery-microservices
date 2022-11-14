using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Queries;
using ECommerce.Services.Identity.Shared;

namespace ECommerce.Services.Identity.Identity.Features.GettingClaims;

public static class GetClaimsEndpoint
{
    internal static IEndpointRouteBuilder MapGetClaimsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{IdentityConfigs.IdentityPrefixUri}/claims", GetClaims)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .Produces<GetClaimsResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDisplayName("Get User claims")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static async Task<IResult> GetClaims(
        IQueryProcessor queryProcessor, CancellationToken cancellationToken)
    {
        var result = await queryProcessor.SendAsync(new GetClaims(), cancellationToken);

        return Results.Ok(result);
    }
}
