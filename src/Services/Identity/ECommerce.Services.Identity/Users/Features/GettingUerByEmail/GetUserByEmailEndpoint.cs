using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Queries;
using ECommerce.Services.Identity.Shared;
using ECommerce.Services.Identity.Users.Features.RegisteringUser;

namespace ECommerce.Services.Identity.Users.Features.GettingUerByEmail;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
public static class GetUserByEmailEndpoint
{
    internal static IEndpointRouteBuilder MapGetUserByEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{UsersConfigs.UsersPrefixUri}/by-email/{{email}}", GetUserByEmail)
            .AllowAnonymous()
            .WithTags(UsersConfigs.Tag)
            .Produces<RegisterUserResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("GetUserByEmail")
            .WithDisplayName("Get User by email.")
            .WithApiVersionSet(SharedModulesConfiguration.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static async Task<IResult> GetUserByEmail(
        [FromRoute]string email,
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken)
    {
        var result = await queryProcessor.SendAsync(new GetUserByEmail(email), cancellationToken);

        return Results.Ok(result);
    }
}
