using BuildingBlocks.Abstractions.CQRS.Command;
using ECommerce.Services.Identity.Users.Features.RegisteringUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Services.Identity.Users.Features.UpdatingUserState;

public static class UpdateUserStateEndpoint
{
    internal static IEndpointRouteBuilder MapUpdateUserStateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut($"{UsersConfigs.UsersPrefixUri}/{{userId:guid}}/state", UpdateUserState)
            .AllowAnonymous()
            .WithTags(UsersConfigs.Tag)
            .Produces<RegisterUserResult>(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Update User State.");

        return endpoints;
    }

    private static async Task<IResult> UpdateUserState(
        Guid userId,
        UpdateUserStateRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserState(userId, request.UserState);

        await commandProcessor.SendAsync(command, cancellationToken);

        return Results.NoContent();
    }
}
