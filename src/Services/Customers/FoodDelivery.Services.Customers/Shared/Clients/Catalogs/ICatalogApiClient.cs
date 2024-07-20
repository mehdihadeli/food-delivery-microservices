using FoodDelivery.Services.Customers.Products.Models;

namespace FoodDelivery.Services.Customers.Shared.Clients.Catalogs;

// https://learn.microsoft.com/en-us/azure/architecture/patterns/anti-corruption-layer
// https://deviq.com/domain-driven-design/anti-corruption-layer

/// <summary>
/// CatalogApiClient acts as a anti-corruption-layer for our system.
/// An Anti-Corruption Layer (ACL) is a set of patterns placed between the domain model and other bounded contexts or third party dependencies. The intent of this layer is to prevent the intrusion of foreign concepts and models into the domain model.
/// </summary>
public interface ICatalogApiClient
{
    Task<Product?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default);
}
