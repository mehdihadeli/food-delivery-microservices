using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messages;

namespace FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;

public record UserStateUpdatedV1(Guid UserId, UserState OldUserState, UserState NewUserState) : IntegrationEvent
{
    /// <summary>
    /// UserStateUpdatedV1 with in-line validation.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="oldUserState"></param>
    /// <param name="newUserState"></param>
    /// <returns></returns>
    public static UserStateUpdatedV1 Of(Guid userId, UserState oldUserState, UserState newUserState) =>
        new(userId.NotBeEmpty(), oldUserState, newUserState);
}
