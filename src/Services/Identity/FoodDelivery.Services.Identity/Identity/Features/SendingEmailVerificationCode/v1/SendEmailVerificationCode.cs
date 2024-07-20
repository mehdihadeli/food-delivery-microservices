using System.Globalization;
using System.Security.Cryptography;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Email;
using FoodDelivery.Services.Identity.Shared.Data;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Identity.Features.SendingEmailVerificationCode.v1;

internal record SendEmailVerificationCode(string Email) : ICommand
{
    public static SendEmailVerificationCode Of(string? email) => new(email.NotBeEmptyOrNull());
}

internal class SendEmailVerificationCodeCommandHandler : ICommandHandler<SendEmailVerificationCode>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SendEmailVerificationCodeCommandHandler> _logger;

    public SendEmailVerificationCodeCommandHandler(
        UserManager<ApplicationUser> userManager,
        IdentityContext context,
        IEmailSender emailSender,
        ILogger<SendEmailVerificationCodeCommandHandler> logger
    )
    {
        _userManager = userManager;
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Unit> Handle(SendEmailVerificationCode command, CancellationToken cancellationToken)
    {
        command.NotBeNull();
        var identityUser = await _userManager.FindByEmailAsync(command.Email);

        identityUser.NotBeNull(new IdentityUserNotFoundException(command.Email));

        if (identityUser.EmailConfirmed)
            throw new ConflictException("Email is already confirmed.");

        bool isExists = await _context
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
            SentAt = DateTime.Now
        };

        await _context.Set<EmailVerificationCode>().AddAsync(emailVerificationCode, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        (string Email, string VerificationCode) model = (command.Email, verificationCode);

        string content =
            $"Welcome to shop application! Please verify your email with using this Code: {model.VerificationCode}.";

        string subject = "Verification Email";

        EmailObject emailObject = new EmailObject(command.Email, subject, content);

        await _emailSender.SendAsync(emailObject);

        _logger.LogInformation("Verification email sent successfully for userId:{UserId}", identityUser.Id);

        return Unit.Value;
    }
}
