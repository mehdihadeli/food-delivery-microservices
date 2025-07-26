using FoodDelivery.ServiceDefaults.Clients.Rest.Catalogs.Dtos;

namespace FoodDelivery.ServiceDefaults.Clients.Rest.Catalogs;

// https://learn.microsoft.com/en-us/azure/architecture/patterns/anti-corruption-layer
// https://deviq.com/domain-driven-design/anti-corruption-layer

/// <summary>
/// CatalogRestClient acts as a anti-corruption-layer for our system.
/// An Anti-Corruption Layer (ACL) is a set of patterns placed between the domain model and other bounded contexts or third party dependencies. The intent of this layer is to prevent the intrusion of foreign concepts and models into the domain model.
/// </summary>
public interface ICatalogClientBase
{
    Task<GetProductByPageClientResponseDto> GetProductByPageAsync(
        GetProductsByPageClientRequestDto getProductsByPageClientRequestDto,
        CancellationToken cancellationToken
    );

    Task<GetProductByIdClientResponseDto?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default);
}

public interface ICatalogsRestClient : ICatalogClientBase;

public interface ICatalogsConnectedServiceClient : ICatalogClientBase;

public interface ICatalogsKiotaClient : ICatalogClientBase;
