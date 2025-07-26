using FoodDelivery.ServiceDefaults.Clients.Rest.Identity.Dtos;

namespace FoodDelivery.ServiceDefaults.Clients.Rest.Identity;

// https://learn.microsoft.com/en-us/azure/architecture/patterns/anti-corruption-layer
// https://deviq.com/domain-driven-design/anti-corruption-layer

/// <summary>
/// IIdentityRestClientBase acts as a anti-corruption-layer for our system.
/// An Anti-Corruption Layer (ACL) is a set of patterns placed between the domain model and other bounded contexts or third party dependencies. The intent of this layer is to prevent the intrusion of foreign concepts and models into the domain model.
/// </summary>
public interface IIdentityClientBase
{
    Task<GetUserByEmailClientResponseDto?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    );

    Task<CreateUserIdentityClientResponseDto?> CreateUserIdentityAsync(
        CreateUserClientRequestDto createUserClientRequestDto,
        CancellationToken cancellationToken = default
    );
}

public interface IIdentityRestClient : IIdentityClientBase;

public interface IIdentityConnectedServiceClientBase : IIdentityClientBase;

public interface IIdentityKiotaClientBase : IIdentityClientBase;
