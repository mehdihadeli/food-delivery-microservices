using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Identity.Users.Features.RegisteringUser.v1;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Users.Features.UpdatingUserState.v1;

public static class UpdateUserStateEndpoint
{
    internal static RouteHandlerBuilder MapUpdateUserStateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/{userId:guid}/state", UpdateUserState)
            .AllowAnonymous()
            .Produces<RegisterUserResponse>(StatusCodes.Status204NoContent)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
            .WithName("UpdateUserState")
            .WithDisplayName("Update User State.")
            .WithMetadata(new SwaggerOperationAttribute("Updating User State.", "Updating User State"));
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
