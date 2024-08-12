using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.v1.Events.Integration;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.V1;

internal record UpdateUserState(Guid UserId, UserState State) : ITxCommand
{
    /// <summary>
    /// UpdateUserState with in-line validation.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static UpdateUserState Of(Guid userId, UserState state)
    {
        return new UpdateUserStateValidator().HandleValidation(new UpdateUserState(userId, state));
    }
}

internal class UpdateUserStateValidator : AbstractValidator<UpdateUserState>
{
    public UpdateUserStateValidator()
    {
        RuleFor(v => v.State).NotEmpty();
        RuleFor(v => v.UserId).NotEmpty();
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
        ILogger<UpdateUserStateHandler> logger
    )
    {
        _bus = bus;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<Unit> Handle(UpdateUserState request, CancellationToken cancellationToken)
    {
        var identityUser = await _userManager.FindByIdAsync(request.UserId.ToString());
        identityUser.NotBeNull(new IdentityUserNotFoundException(request.UserId));

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

        var userStateUpdated = UserStateUpdated.Of(
            request.UserId,
            (UserState)(int)previousState,
            (UserState)(int)request.State
        );

        await _bus.PublishAsync(userStateUpdated, null, cancellationToken);

        _logger.LogInformation(
            "Updated state for user with ID: '{UserId}', '{PreviousState}' -> '{UserState}'",
            identityUser.Id,
            previousState,
            identityUser.UserState
        );

        return Unit.Value;
    }
}
