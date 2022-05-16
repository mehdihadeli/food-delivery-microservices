using BuildingBlocks.Abstractions.CQRS.Query;
using ECommerce.Services.Identity.Users.Features.RegisteringUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Services.Identity.Users.Features.GettingUserById;

public static class GetUserByIdEndpoint
{
    internal static IEndpointRouteBuilder MapGetUserByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{UsersConfigs.UsersPrefixUri}/{{userId:guid}}", GetUserById)
            .AllowAnonymous()
            .WithTags(UsersConfigs.Tag)
            .Produces<RegisterUserResult>(StatusCodes.Status200OK)
            .Produces<RegisterUserResult>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("GetUserById")
            .WithDisplayName("Get User by Id.");

        return endpoints;
    }

    private static async Task<IResult> GetUserById(
        Guid userId,
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken)
    {
        var result = await queryProcessor.SendAsync(new GetUserById(userId), cancellationToken);

        return Results.Ok(result);
    }
}
