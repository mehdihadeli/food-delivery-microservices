using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Services.Shared.Customers.Customers.Events.v1.Integration;

public record CustomerUpdatedV1(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid IdentityId,
    DateTime CreatedAt,
    DateTime? BirthDate = null,
    string? Nationality = null,
    string? Address = null
) : IntegrationEvent
{
    /// <summary>
    /// CustomerUpdatedV1 with in-line validation.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <param name="email"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="identityId"></param>
    /// <param name="createdAt"></param>
    /// <param name="birthDate"></param>
    /// <param name="nationality"></param>
    /// <param name="address"></param>
    /// <returns></returns>
    public static CustomerUpdatedV1 Of(
        long id,
        string? firstName,
        string? lastName,
        string? email,
        string? phoneNumber,
        Guid identityId,
        DateTime createdAt,
        DateTime? birthDate,
        string? nationality,
        string? address
    )
    {
        id.NotBeNegativeOrZero();
        firstName.NotBeNullOrWhiteSpace();
        lastName.NotBeNullOrWhiteSpace();
        email.NotBeNullOrWhiteSpace().NotBeInvalidEmail();
        phoneNumber.NotBeNullOrWhiteSpace();
        identityId.NotBeEmpty();

        return new CustomerUpdatedV1(
            id,
            firstName,
            lastName,
            email,
            phoneNumber,
            identityId,
            createdAt,
            birthDate,
            nationality,
            address
        );
    }
}
