using FoodDelivery.Services.Customers.Products.Models;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs.Dtos;
using FoodDelivery.Services.Customers.Shared.Clients.Identity.Dtos;
using FoodDelivery.Services.Customers.Users.Model;
using Riok.Mapperly.Abstractions;

namespace FoodDelivery.Services.Customers.Shared.Clients;

[Mapper]
internal static partial class ClientsMappings
{
    internal static partial Product ToProduct(this ProductClientDto productClientDto);

    internal static partial UserIdentity ToUserIdentity(this IdentityUserClientDto identityUserClientDto);
}
