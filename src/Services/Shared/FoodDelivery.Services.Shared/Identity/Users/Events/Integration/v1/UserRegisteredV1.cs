using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messages;

namespace FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;

public record UserRegisteredV1(
    Guid IdentityId,
    string Email,
    string PhoneNumber,
    string UserName,
    string FirstName,
    string LastName,
    IEnumerable<string>? Roles
) : IntegrationEvent
{
    /// <summary>
    /// UserRegisteredV1 with in-line validation.
    /// </summary>
    /// <param name="identityId"></param>
    /// <param name="email"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="userName"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    public static UserRegisteredV1 Of(
        Guid identityId,
        string? email,
        string? phoneNumber,
        string? userName,
        string? firstName,
        string? lastName,
        IEnumerable<string>? roles
    )
    {
        return new UserRegisteredV1(
            identityId.NotBeEmpty(),
            email.NotBeEmptyOrNull().NotBeInvalidEmail(),
            phoneNumber.NotBeEmptyOrNull(),
            userName.NotBeEmptyOrNull(),
            firstName.NotBeEmptyOrNull(),
            lastName.NotBeEmptyOrNull(),
            roles
        );
    }
}
