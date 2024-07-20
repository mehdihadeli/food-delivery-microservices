using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.Shared.Data;

namespace FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;

public class CustomersReadUnitOfWork : MongoUnitOfWork<CustomersReadDbContext>, ICustomersReadUnitOfWork
{
    public CustomersReadUnitOfWork(
        CustomersReadDbContext context,
        IRestockSubscriptionReadRepository restockRepository,
        ICustomerReadRepository customerRepository
    )
        : base(context)
    {
        RestockSubscriptionsRepository = restockRepository;
        CustomersRepository = customerRepository;
    }

    public IRestockSubscriptionReadRepository RestockSubscriptionsRepository { get; }
    public ICustomerReadRepository CustomersRepository { get; }
}
