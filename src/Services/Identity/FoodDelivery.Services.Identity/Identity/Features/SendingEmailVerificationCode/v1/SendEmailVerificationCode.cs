using System.Globalization;
using System.Security.Cryptography;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Email;
using FoodDelivery.Services.Identity.Shared.Data;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ICommand = BuildingBlocks.Abstractions.Commands.ICommand;

namespace FoodDelivery.Services.Identity.Identity.Features.SendingEmailVerificationCode.v1;

public record SendEmailVerificationCode(string Email) : ICommand
{
    public static SendEmailVerificationCode Of(string? email) => new(email.NotBeEmptyOrNull());
}

public class SendEmailVerificationCodeCommandHandler(
    UserManager<ApplicationUser> userManager,
    IdentityContext context,
    IEmailSender emailSender,
    ILogger<SendEmailVerificationCodeCommandHandler> logger
) : BuildingBlocks.Abstractions.Commands.ICommandHandler<SendEmailVerificationCode>
{
    public async ValueTask<Unit> Handle(SendEmailVerificationCode command, CancellationToken cancellationToken)
    {
        command.NotBeNull();
        var identityUser = await userManager.FindByEmailAsync(command.Email);

        identityUser.NotBeNull(new IdentityUserNotFoundException(command.Email));

        if (identityUser.EmailConfirmed)
            throw new ConflictException("Email is already confirmed.");

        bool isExists = await context
            .Set<EmailVerificationCode>()
            .AnyAsync(evc => evc.Email == command.Email && evc.SentAt.AddMinutes(5) > DateTime.Now, cancellationToken);

        if (isExists)
        {
            throw new BadRequestException(
                "You already have an active code. Please wait! You may receive the code in your email. If not, please try again after sometimes."
            );
        }

        int randomNumber = RandomNumberGenerator.GetInt32(0, 1000000);
        string verificationCode = randomNumber.ToString("D6", CultureInfo.InvariantCulture);

        EmailVerificationCode emailVerificationCode = new EmailVerificationCode()
        {
            Code = verificationCode,
            Email = command.Email,
            SentAt = DateTime.Now,
        };

        await context.Set<EmailVerificationCode>().AddAsync(emailVerificationCode, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        (string Email, string VerificationCode) model = (command.Email, verificationCode);

        string content =
            $"Welcome to shop application! Please verify your email with using this Code: {model.VerificationCode}.";

        string subject = "Verification Email";

        EmailObject emailObject = new EmailObject(command.Email, subject, content);

        await emailSender.SendAsync(emailObject);

        logger.LogInformation("Verification email sent successfully for userId:{UserId}", identityUser.Id);

        return Unit.Value;
    }
}
