using BuildingBlocks.Core.Messaging;

namespace Store.Services.Shared.Identity.Users.Events.Integration;

public record UserStateUpdated(Guid UserId, UserState OldUserState, UserState NewUserState) : IntegrationEvent;

public enum UserState
{
    Active = 1,
    Locked = 2
}
