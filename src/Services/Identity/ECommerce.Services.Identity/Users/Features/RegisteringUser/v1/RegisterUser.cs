using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using ECommerce.Services.Identity.Shared.Models;
using ECommerce.Services.Identity.Users.Dtos;
using ECommerce.Services.Identity.Users.Dtos.v1;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using UserState = ECommerce.Services.Identity.Shared.Models.UserState;

namespace ECommerce.Services.Identity.Users.Features.RegisteringUser.v1;

public record RegisterUser(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword,
    List<string>? Roles = null) : ITxCreateCommand<RegisterUserResponse>
{
    public DateTime CreatedAt { get; init; } = DateTime.Now;
}

internal class RegisterUserValidator : AbstractValidator<RegisterUser>
{
    public RegisterUserValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(v => v.FirstName)
            .NotEmpty()
            .WithMessage("FirstName is required.");

        RuleFor(v => v.LastName)
            .NotEmpty()
            .WithMessage("LastName is required.");

        RuleFor(v => v.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress();

        RuleFor(v => v.UserName)
            .NotEmpty()
            .WithMessage("UserName is required.");

        RuleFor(v => v.Password)
            .NotEmpty()
            .WithMessage("Password is required.");

        RuleFor(v => v.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("The password and confirmation password do not match.")
            .NotEmpty();

        RuleFor(v => v.Roles).Custom((roles, c) =>
        {
            if (roles != null &&
                !roles.All(x => x.Contains(IdentityConstants.Role.Admin, StringComparison.Ordinal) ||
                                x.Contains(IdentityConstants.Role.User, StringComparison.Ordinal)))
            {
                c.AddFailure("Invalid roles.");
            }
        });
    }
}

// using transaction script instead of using domain business logic here
// https://www.youtube.com/watch?v=PrJIMTZsbDw
internal class RegisterUserHandler : ICommandHandler<RegisterUser, RegisterUserResponse>
{
    private readonly IMessagePersistenceService _messagePersistenceService;

    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterUserHandler(
        UserManager<ApplicationUser> userManager,
        IMessagePersistenceService messagePersistenceService)
    {
        _messagePersistenceService = messagePersistenceService;
        _userManager = Guard.Against.Null(userManager, nameof(userManager));
    }

    public async Task<RegisterUserResponse> Handle(RegisterUser request, CancellationToken cancellationToken)
    {
        var applicationUser = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            Email = request.Email,
            UserState = UserState.Active,
            CreatedAt = request.CreatedAt,
        };

        var identityResult = await _userManager.CreateAsync(applicationUser, request.Password);
        if (!identityResult.Succeeded)
            throw new RegisterIdentityUserException(string.Join(',', identityResult.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRolesAsync(
            applicationUser,
            request.Roles ?? new List<string> {IdentityConstants.Role.User});

        if (!roleResult.Succeeded)
            throw new RegisterIdentityUserException(string.Join(',', roleResult.Errors.Select(e => e.Description)));


        var userRegistered = new UserRegisteredV1(
            applicationUser.Id,
            applicationUser.Email,
            applicationUser.UserName,
            applicationUser.FirstName,
            applicationUser.LastName,
            request.Roles);

        // publish our integration event and save to outbox should do in same transaction of our business logic actions. we could use TxBehaviour or ITxDbContextExecutes interface
        // This service is not DDD, so we couldn't use DomainEventPublisher to publish mapped integration events
        await _messagePersistenceService.AddPublishMessageAsync(
            new MessageEnvelope<UserRegisteredV1>(userRegistered, new Dictionary<string, object?>()), cancellationToken);

        return new RegisterUserResponse(new IdentityUserDto
        {
            Id = applicationUser.Id,
            Email = applicationUser.Email,
            UserName = applicationUser.UserName,
            FirstName = applicationUser.FirstName,
            LastName = applicationUser.LastName,
            Roles = request.Roles ?? new List<string> {IdentityConstants.Role.User},
            RefreshTokens = applicationUser?.RefreshTokens?.Select(x => x.Token),
            CreatedAt = request.CreatedAt,
            UserState = UserState.Active
        });
    }
}
