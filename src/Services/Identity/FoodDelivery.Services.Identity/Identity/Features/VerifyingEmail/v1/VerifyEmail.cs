using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Identity.Features.VerifyingEmail.v1.Exceptions;
using FoodDelivery.Services.Identity.Shared.Data;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
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
        command.NotBeNull();

        var user = await userManager.FindByEmailAsync(command.Email);
        user.NotBeNull(new IdentityUserNotFoundException(command.Email));

        if (user.EmailConfirmed)
        {
            throw new EmailAlreadyVerifiedException(user.Email!);
        }

        var emailVerificationCode = await dbContext
            .Set<EmailVerificationCode>()
            .Where(x => x.Email == command.Email && x.Code == command.Code && x.UsedAt == null)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (emailVerificationCode == null)
        {
            throw new BadRequestException("Either email or code is incorrect.");
        }

        if (DateTime.Now > emailVerificationCode.SentAt.AddMinutes(5))
        {
            throw new BadRequestException("The code is expired.");
        }

        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        emailVerificationCode.UsedAt = DateTime.Now;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Email verified successfully for userId:{UserId}", user.Id);

        return Unit.Value;
    }
}
