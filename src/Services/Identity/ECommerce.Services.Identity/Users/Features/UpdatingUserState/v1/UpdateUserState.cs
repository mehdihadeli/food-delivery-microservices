using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Identity.Shared.Exceptions;
using ECommerce.Services.Identity.Shared.Models;
using ECommerce.Services.Identity.Users.Features.UpdatingUserState.v1.Events.Integration;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Services.Identity.Users.Features.UpdatingUserState.v1;

public record UpdateUserState(Guid UserId, UserState State) : ITxUpdateCommand;

internal class UpdateUserStateValidator : AbstractValidator<UpdateUserState>
{
    public UpdateUserStateValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(v => v.State)
            .NotEmpty();

        RuleFor(v => v.UserId)
            .NotEmpty();
    }
}

internal class UpdateUserStateHandler : ICommandHandler<UpdateUserState>
{
    private readonly IBus _bus;
    private readonly ILogger<UpdateUserStateHandler> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserStateHandler(
        IBus bus,
        UserManager<ApplicationUser> userManager,
        ILogger<UpdateUserStateHandler> logger)
    {
        _bus = bus;
        _logger = logger;
        _userManager = Guard.Against.Null(userManager, nameof(userManager));
    }

    public async Task<Unit> Handle(UpdateUserState request, CancellationToken cancellationToken)
    {
        var identityUser = await _userManager.FindByIdAsync(request.UserId.ToString());
        Guard.Against.NotFound(identityUser, new IdentityUserNotFoundException(request.UserId));

        var previousState = identityUser!.UserState;
        if (previousState == request.State)
        {
            return Unit.Value;
        }

        if (await _userManager.IsInRoleAsync(identityUser, IdentityConstants.Role.Admin))
        {
            throw new UserStateCannotBeChangedException(request.State, request.UserId);
        }

        identityUser.UserState = request.State;

        await _userManager.UpdateAsync(identityUser);

        var userStateUpdated = new UserStateUpdated(
            request.UserId,
            (UserState)(int)previousState,
            (UserState)(int)request.State);

        await _bus.PublishAsync(userStateUpdated, null, cancellationToken);


        _logger.LogInformation(
            "Updated state for user with ID: '{UserId}', '{PreviousState}' -> '{UserState}'",
            identityUser.Id,
            previousState,
            identityUser.UserState);

        return Unit.Value;
    }
}
