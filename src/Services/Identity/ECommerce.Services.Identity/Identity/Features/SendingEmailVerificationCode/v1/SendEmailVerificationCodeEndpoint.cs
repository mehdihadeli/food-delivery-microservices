using BuildingBlocks.Abstractions.CQRS.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Identity.Features.SendingEmailVerificationCode.v1;

public static class SendEmailVerificationCodeEndpoint
{
    internal static RouteHandlerBuilder MapSendEmailVerificationCodeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/send-email-verification-code", SendEmailVerificationCode)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("SendEmailVerificationCode")
            .WithDisplayName("Send Email Verification Code.")
            .WithMetadata(new SwaggerOperationAttribute(
                "Sending Email Verification Code.",
                "Sending Email Verification Code."));
    }

    private static async Task<IResult> SendEmailVerificationCode(
        SendEmailVerificationCodeRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        var command = new SendEmailVerificationCode(request.Email);

        var result = await commandProcessor.SendAsync(command, cancellationToken);

        return Results.Ok(result);
    }
}
