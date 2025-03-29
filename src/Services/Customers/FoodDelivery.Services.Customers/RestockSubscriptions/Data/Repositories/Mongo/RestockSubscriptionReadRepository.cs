using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.Shared.Data;
using Sieve.Services;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Data.Repositories.Mongo;

public class RestockSubscriptionReadRepository(CustomersReadDbContext context, ISieveProcessor sieveProcessor)
    : MongoRepositoryBase<CustomersReadDbContext, RestockSubscriptionReadModel, Guid>(context, sieveProcessor),
        IRestockSubscriptionReadRepository;
