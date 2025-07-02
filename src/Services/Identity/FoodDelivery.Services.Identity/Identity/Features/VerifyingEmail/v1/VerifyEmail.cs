using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Identity.Features.VerifyingEmail.v1.Exceptions;
using FoodDelivery.Services.Identity.Shared.Data;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Shared.Identity.Users;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ICommand = BuildingBlocks.Abstractions.Commands.ICommand;

namespace FoodDelivery.Services.Identity.Identity.Features.VerifyingEmail.v1;

public record VerifyEmail(string Email, string Code) : ICommand
{
    /// <summary>
    /// Verify email with in-line validation.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static VerifyEmail Of(string? email, string? code) =>
        new(email.NotBeEmptyOrNull().NotBeInvalidEmail(), code.NotBeEmptyOrNull());
}

public class VerifyEmailHandler(
    UserManager<ApplicationUser> userManager,
    IdentityContext dbContext,
    ILogger<VerifyEmailHandler> logger
) : BuildingBlocks.Abstractions.Commands.ICommandHandler<VerifyEmail>
{
    public async ValueTask<Unit> Handle(VerifyEmail command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
        {
            logger.LogWarning("Email verification failed: User with email {Email} not found", command.Email);
            throw new BadRequestException("User not found");
        }

        // Verify the email confirmation token
        var result = await userManager.ConfirmEmailAsync(user, command.Code);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Email verification failed for {Email}: {Errors}", command.Email, errors);
            throw new BadRequestException($"Email verification failed: {errors}");
        }

        // Optional: Update user state or other properties
        user.EmailConfirmed = true;
        user.UserState = UserState.Active;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Email {Email} successfully verified", command.Email);

        return Unit.Value;
    }
}
