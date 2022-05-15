using BuildingBlocks.Core.Messaging;
using Store.Services.Identity.Shared.Models;

namespace Store.Services.Identity.Users.Features.UpdatingUserState.Events.Integration;

public record UserStateUpdated(Guid UserId, UserState OldUserState, UserState NewUserState) : IntegrationEvent;
