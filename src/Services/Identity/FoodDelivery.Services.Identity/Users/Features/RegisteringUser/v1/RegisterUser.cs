using System.Security.Claims;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Core.Security;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Dtos.v1;
using FoodDelivery.Services.Shared;
using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;
using Microsoft.AspNetCore.Identity;
using UserState = FoodDelivery.Services.Shared.Identity.Users.UserState;

namespace FoodDelivery.Services.Identity.Users.Features.RegisteringUser.v1;

public record RegisterUser(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string PhoneNumber,
    string Password,
    string ConfirmPassword,
    IEnumerable<string> Roles,
    IEnumerable<string>? Permissions
) : ITxCommand<RegisterUserResult>
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
    /// <param name="permissions"></param>
    /// <returns></returns>
    public static RegisterUser Of(
        string firstName,
        string lastName,
        string userName,
        string email,
        string phoneNumber,
        string password,
        string confirmPassword,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions
    )
    {
        return new RegisterUserValidator().HandleValidation(
            new RegisterUser(
                firstName,
                lastName,
                userName,
                email,
                phoneNumber,
                password,
                confirmPassword,
                roles,
                permissions
            )
        );
    }
}

public class RegisterUserValidator : AbstractValidator<RegisterUser>
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
            .MaximumLength(20)
            .WithMessage("PhoneNumber must not exceed 20 characters.");
        RuleFor(v => v.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("The password and confirmation password do not match.")
            .NotEmpty();

        RuleFor(v => v.Roles).NotEmpty();
        RuleFor(v => v.Roles)
            .Custom(
                (roles, c) =>
                {
                    if (
                        roles != null
                        && !roles.All(x =>
                            x.Contains(Authorization.Roles.Admin, StringComparison.Ordinal)
                            || x.Contains(Authorization.Roles.User, StringComparison.Ordinal)
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
public class RegisterUserHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IExternalEventBus externalEventBus
) : ICommandHandler<RegisterUser, RegisterUserResult>
{
    public async ValueTask<RegisterUserResult> Handle(RegisterUser request, CancellationToken cancellationToken)
    {
        // Create the user
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

        // Validate that all requested roles exist
        var rolesToAssign = request.Roles.ToList();
        await ValidateRolesExist(rolesToAssign);

        // Add user to roles
        var roleResult = await userManager.AddToRolesAsync(applicationUser, rolesToAssign);
        if (!roleResult.Succeeded)
            throw new RegisterIdentityUserException(string.Join(',', roleResult.Errors.Select(e => e.Description)));

        // Add user-specific permissions if any
        if (request.Permissions != null && request.Permissions.Any())
        {
            var permissionClaims = request
                .Permissions.Select(permission => new Claim(ClaimsType.Permission, permission))
                .ToList();

            var addClaimsResult = await userManager.AddClaimsAsync(applicationUser, permissionClaims);
            if (!addClaimsResult.Succeeded)
            {
                throw new RegisterIdentityUserException(
                    string.Join(',', addClaimsResult.Errors.Select(e => e.Description))
                );
            }
        }

        var userRegistered = UserRegisteredV1.Of(
            applicationUser.Id,
            applicationUser.Email,
            applicationUser.PhoneNumber!,
            applicationUser.UserName,
            applicationUser.FirstName,
            applicationUser.LastName,
            rolesToAssign,
            request.Permissions
        );

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
                Roles = rolesToAssign,
                CreatedAt = request.CreatedAt,
                UserState = UserState.Active,
            }
        );
    }

    private async Task ValidateRolesExist(IEnumerable<string> roleNames)
    {
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                throw new InvalidOperationException($"Role '{roleName}' does not exist in the system");
            }
        }
    }
}

public record RegisterUserResult(IdentityUserDto? UserIdentity);
