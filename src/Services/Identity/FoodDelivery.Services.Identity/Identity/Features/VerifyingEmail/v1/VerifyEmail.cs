using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Identity.Identity.Features.VerifyingEmail.v1.Exceptions;
using FoodDelivery.Services.Identity.Shared.Data;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

internal class VerifyEmailHandler : ICommandHandler<VerifyEmail>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityContext _dbContext;
    private readonly ILogger<VerifyEmailHandler> _logger;

    public VerifyEmailHandler(
        UserManager<ApplicationUser> userManager,
        IdentityContext dbContext,
        ILogger<VerifyEmailHandler> logger
    )
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Unit> Handle(VerifyEmail command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var user = await _userManager.FindByEmailAsync(command.Email);
        user.NotBeNull(new IdentityUserNotFoundException(command.Email));

        if (user.EmailConfirmed)
        {
            throw new EmailAlreadyVerifiedException(user.Email!);
        }

        var emailVerificationCode = await _dbContext
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
        await _userManager.UpdateAsync(user);

        emailVerificationCode.UsedAt = DateTime.Now;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email verified successfully for userId:{UserId}", user.Id);

        return Unit.Value;
    }
}
