using BuildingBlocks.Abstractions.CQRS.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Identity.Features.VerifyingEmail.v1;

public static class VerifyEmailEndpoint
{
    internal static RouteHandlerBuilder MapSendVerifyEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/verify-email", VerifyEmail)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("VerifyEmail")
            .WithDisplayName("Verify Email.")
            .WithMetadata(new SwaggerOperationAttribute(
                "Verifying Email",
                "Verifying Email"));
    }

    private static async Task<IResult> VerifyEmail(
        VerifyEmailRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmail(request.Email, request.Code);

        await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Ok();
    }
}
