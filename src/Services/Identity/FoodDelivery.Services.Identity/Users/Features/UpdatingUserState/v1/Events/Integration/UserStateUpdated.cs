using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;
using FoodDelivery.Services.Identity.Shared.Models;

namespace FoodDelivery.Services.Identity.Users.Features.UpdatingUserState.v1.Events.Integration;

public record UserStateUpdated(Guid UserId, UserState OldUserState, UserState NewUserState) : IntegrationEvent
{
    /// <summary>
    /// UserStateUpdated with in-line validation.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="oldUserState"></param>
    /// <param name="newUserState"></param>
    /// <returns></returns>
    public static UserStateUpdated Of(Guid userId, UserState oldUserState, UserState newUserState) =>
        new(userId.NotBeInvalid(), oldUserState, newUserState);
}
