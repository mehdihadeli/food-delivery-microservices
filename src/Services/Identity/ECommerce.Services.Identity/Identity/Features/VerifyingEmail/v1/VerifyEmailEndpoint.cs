using BuildingBlocks.Abstractions.CQRS.Commands;
using Hellang.Middleware.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Identity.Features.VerifyingEmail.v1;

public static class VerifyEmailEndpoint
{
    internal static RouteHandlerBuilder MapSendVerifyEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/verify-email", VerifyEmail)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<StatusCodeProblemDetails>(StatusCodes.Status400BadRequest)
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
