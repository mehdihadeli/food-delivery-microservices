using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Identity.Shared.Exceptions;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.v1.Events.Integration;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.v1;

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

internal class UpdateUserStateHandler(
    IExternalEventBus bus,
    UserManager<ApplicationUser> userManager,
    ILogger<UpdateUserStateHandler> logger
) : ICommandHandler<UpdateUserState>
{
    public async Task Handle(UpdateUserState request, CancellationToken cancellationToken)
    {
        var identityUser = await userManager.FindByIdAsync(request.UserId.ToString());
        identityUser.NotBeNull(new IdentityUserNotFoundException(request.UserId));

        var previousState = identityUser!.UserState;
        if (previousState == request.State)
        {
            return;
        }

        if (await userManager.IsInRoleAsync(identityUser, IdentityConstants.Role.Admin))
        {
            throw new UserStateCannotBeChangedException(request.State, request.UserId);
        }

        identityUser.UserState = request.State;

        await userManager.UpdateAsync(identityUser);

        var userStateUpdated = UserStateUpdated.Of(
            request.UserId,
            (UserState)(int)previousState,
            (UserState)(int)request.State
        );

        await bus.PublishAsync(userStateUpdated, cancellationToken);

        logger.LogInformation(
            "Updated state for user with ID: '{UserId}', '{PreviousState}' -> '{UserState}'",
            identityUser.Id,
            previousState,
            identityUser.UserState
        );
    }
}
