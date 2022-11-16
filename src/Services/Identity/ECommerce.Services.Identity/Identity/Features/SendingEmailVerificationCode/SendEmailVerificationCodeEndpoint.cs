using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Identity.Shared;

namespace ECommerce.Services.Identity.Identity.Features.SendingEmailVerificationCode;

public static class SendEmailVerificationCodeEndpoint
{
    internal static IEndpointRouteBuilder MapSendEmailVerificationCodeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                $"{IdentityConfigs.IdentityPrefixUri}/send-email-verification-code",
                SendEmailVerificationCode)
            .WithTags(IdentityConfigs.Tag)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("SendEmailVerificationCode")
            .WithDisplayName("Send Email Verification Code.")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
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
