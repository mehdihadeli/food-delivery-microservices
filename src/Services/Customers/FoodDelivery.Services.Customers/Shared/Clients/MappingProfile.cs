using AutoMapper;
using FoodDelivery.Services.Customers.Products.Models;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs.Dtos;
using FoodDelivery.Services.Customers.Shared.Clients.Identity.Dtos;
using FoodDelivery.Services.Customers.Users.Model;

namespace FoodDelivery.Services.Customers.Shared.Clients;

public class ClientsMappingProfile : Profile
{
    public ClientsMappingProfile()
    {
        CreateMap<ProductClientDto, Product>();
        CreateMap<IdentityUserClientDto, UserIdentity>();
    }
}
