using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.Shared.Data;
using Sieve.Services;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Data.Repositories.Mongo;

public class RestockSubscriptionReadRepository
    : MongoRepositoryBase<CustomersReadDbContext, RestockSubscription, Guid>,
        IRestockSubscriptionReadRepository
{
    public RestockSubscriptionReadRepository(CustomersReadDbContext context, ISieveProcessor sieveProcessor)
        : base(context, sieveProcessor) { }
}
