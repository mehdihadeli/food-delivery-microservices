using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.Shared.Data;

namespace FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;

public class CustomersReadUnitOfWork(
    CustomersReadDbContext context,
    IRestockSubscriptionReadRepository restockRepository,
    ICustomerReadRepository customerRepository
) : MongoUnitOfWork<CustomersReadDbContext>(context), ICustomersReadUnitOfWork
{
    public IRestockSubscriptionReadRepository RestockSubscriptionsRepository { get; } = restockRepository;
    public ICustomerReadRepository CustomersRepository { get; } = customerRepository;
}
