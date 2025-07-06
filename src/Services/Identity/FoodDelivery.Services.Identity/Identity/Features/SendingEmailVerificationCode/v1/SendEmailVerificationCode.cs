using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Email;
using FoodDelivery.Services.Identity.Shared.Models;
using Mediator;
using Microsoft.AspNetCore.Identity;
using ICommand = BuildingBlocks.Abstractions.Commands.ICommand;

namespace FoodDelivery.Services.Identity.Identity.Features.SendingEmailVerificationCode.v1;

public record SendEmailVerificationCode(string Email) : ICommand
{
    public static SendEmailVerificationCode Of(string? email) => new(email.NotBeEmptyOrNull());
}

public class SendEmailVerificationCodeCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    ILogger<SendEmailVerificationCodeCommandHandler> logger
) : BuildingBlocks.Abstractions.Commands.ICommandHandler<SendEmailVerificationCode>
{
    public async ValueTask<Unit> Handle(SendEmailVerificationCode command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
        {
            logger.LogWarning(
                "Email verification code request failed: User with email {Email} not found",
                command.Email
            );
            throw new BadRequestException("User not found");
        }

        if (user.EmailConfirmed)
        {
            logger.LogWarning("Email {Email} is already verified", command.Email);
            throw new BadRequestException("Email is already verified");
        }

        // Generate email confirmation token
        var verificationCode = await userManager.GenerateEmailConfirmationTokenAsync(user);

        (string Email, string VerificationCode) model = (command.Email, verificationCode);

        string content =
            $"Welcome to shop application! Please verify your email with using this Code: {model.VerificationCode}.";

        string subject = "Verification Email";

        EmailObject emailObject = new EmailObject(command.Email, subject, content);

        await emailSender.SendAsync(emailObject);

        logger.LogInformation("Verification email sent successfully for userId:{UserId}", user.Id);

        return Unit.Value;
    }
}
