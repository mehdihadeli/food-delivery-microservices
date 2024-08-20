using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using FoodDelivery.Services.Shared.Identity.Users.Events.V1.Integration;
using Microsoft.AspNetCore.Identity;
using UserState = FoodDelivery.Services.Identity.Shared.Models.UserState;

namespace FoodDelivery.Services.Identity.Users.Features.RegisteringUser.v1;

public record RegisterUser(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string PhoneNumber,
    string Password,
    string ConfirmPassword,
    IEnumerable<string>? Roles = null
) : ITxCreateCommand<RegisterUserResult>
{
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// RegisterUser with in-line validation.
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <param name="userName"></param>
    /// <param name="email"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="password"></param>
    /// <param name="confirmPassword"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    public static RegisterUser Of(
        string? firstName,
        string? lastName,
        string? userName,
        string? email,
        string? phoneNumber,
        string? password,
        string? confirmPassword,
        IEnumerable<string>? roles = null
    )
    {
        return new RegisterUserValidator().HandleValidation(
            new RegisterUser(firstName!, lastName!, userName!, email!, phoneNumber!, password!, confirmPassword!, roles)
        );
    }
}

internal class RegisterUserValidator : AbstractValidator<RegisterUser>
{
    public RegisterUserValidator()
    {
        RuleFor(v => v.FirstName).NotEmpty().WithMessage("FirstName is required.");
        RuleFor(v => v.LastName).NotEmpty().WithMessage("LastName is required.");
        RuleFor(v => v.Email).NotEmpty().WithMessage("Email is required.").EmailAddress();
        RuleFor(v => v.UserName).NotEmpty().WithMessage("UserName is required.");
        RuleFor(v => v.Password).NotEmpty().WithMessage("Password is required.");
        RuleFor(p => p.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone Number is required.")
            .MinimumLength(7)
            .WithMessage("PhoneNumber must not be less than 7 characters.")
            .MaximumLength(15)
            .WithMessage("PhoneNumber must not exceed 15 characters.");
        RuleFor(v => v.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("The password and confirmation password do not match.")
            .NotEmpty();
        RuleFor(v => v.Roles)
            .Custom(
                (roles, c) =>
                {
                    if (
                        roles != null
                        && !roles.All(x =>
                            x.Contains(IdentityConstants.Role.Admin, StringComparison.Ordinal)
                            || x.Contains(IdentityConstants.Role.User, StringComparison.Ordinal)
                        )
                    )
                    {
                        c.AddFailure("Invalid roles.");
                    }
                }
            );
    }
}

// using transaction script instead of using domain business logic here
// https://www.youtube.com/watch?v=PrJIMTZsbDw
internal class RegisterUserHandler(UserManager<ApplicationUser> userManager, IExternalEventBus externalEventBus)
    : ICommandHandler<RegisterUser, RegisterUserResult>
{
    public async Task<RegisterUserResult> Handle(RegisterUser request, CancellationToken cancellationToken)
    {
        var applicationUser = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            UserState = UserState.Active,
            CreatedAt = request.CreatedAt,
        };

        var identityResult = await userManager.CreateAsync(applicationUser, request.Password);
        if (!identityResult.Succeeded)
            throw new RegisterIdentityUserException(string.Join(',', identityResult.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRolesAsync(
            applicationUser,
            request.Roles ?? new List<string> { IdentityConstants.Role.User }
        );

        if (!roleResult.Succeeded)
            throw new RegisterIdentityUserException(string.Join(',', roleResult.Errors.Select(e => e.Description)));

        var userRegistered = UserRegisteredV1.Of(
            applicationUser.Id,
            applicationUser.Email,
            applicationUser.PhoneNumber!,
            applicationUser.UserName,
            applicationUser.FirstName,
            applicationUser.LastName,
            request.Roles
        );

        // publish our integration event and save to outbox should do in same transaction of our business logic actions. we could use TxBehaviour or ITxDbContextExecutes interface
        // This service is not DDD, so we couldn't use DomainEventPublisher to publish mapped integration events
        await externalEventBus.PublishAsync(userRegistered, cancellationToken);

        return new RegisterUserResult(
            new IdentityUserDto
            {
                Id = applicationUser.Id,
                Email = applicationUser.Email,
                PhoneNumber = applicationUser.PhoneNumber,
                UserName = applicationUser.UserName,
                FirstName = applicationUser.FirstName,
                LastName = applicationUser.LastName,
                Roles = request.Roles ?? new List<string> { IdentityConstants.Role.User },
                RefreshTokens = applicationUser?.RefreshTokens?.Select(x => x.Token),
                CreatedAt = request.CreatedAt,
                UserState = UserState.Active
            }
        );
    }
}

internal record RegisterUserResult(IdentityUserDto? UserIdentity);
