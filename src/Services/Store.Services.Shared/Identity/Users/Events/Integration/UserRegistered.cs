using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Messaging;

namespace Store.Services.Shared.Identity.Users.Events.Integration;

public record UserRegistered(
    Guid IdentityId,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    List<string>? Roles) : IntegrationEvent;
