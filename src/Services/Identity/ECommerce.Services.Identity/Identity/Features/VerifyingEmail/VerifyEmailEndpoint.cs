using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Identity.Shared;

namespace ECommerce.Services.Identity.Identity.Features.VerifyingEmail;

public static class VerifyEmailEndpoint
{
    internal static IEndpointRouteBuilder MapSendVerifyEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                $"{IdentityConfigs.IdentityPrefixUri}/verify-email", VerifyEmail)
            .WithTags(IdentityConfigs.Tag)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("VerifyEmail")
            .WithDisplayName("Verify Email.")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
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
