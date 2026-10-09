using FoodDelivery.Services.Customers.Customers.Models.Reads;
using Squirrel.Abstractions.Persistence;

namespace FoodDelivery.Services.Customers.Shared.Contracts;

public interface ICustomerReadRepository : IRepository<CustomerReadModel, Guid> { }
