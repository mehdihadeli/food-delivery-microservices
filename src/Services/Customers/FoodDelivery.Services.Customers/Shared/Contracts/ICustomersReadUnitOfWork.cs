using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Customers.Shared.Data;

namespace FoodDelivery.Services.Customers.Shared.Contracts;

public interface ICustomersReadUnitOfWork : IUnitOfWork<CustomersReadDbContext>
{
    public IRestockSubscriptionReadRepository RestockSubscriptionsRepository { get; }
    public ICustomerReadRepository CustomersRepository { get; }
}
