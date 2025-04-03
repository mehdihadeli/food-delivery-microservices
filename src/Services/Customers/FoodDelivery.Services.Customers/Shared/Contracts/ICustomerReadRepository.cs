using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Customers.Customers.Models.Reads;

namespace FoodDelivery.Services.Customers.Shared.Contracts;

public interface ICustomerReadRepository : IRepository<CustomerReadModel, Guid> { }
