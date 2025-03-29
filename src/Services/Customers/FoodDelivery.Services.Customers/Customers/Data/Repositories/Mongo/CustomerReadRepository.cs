using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.Shared.Data;
using Sieve.Services;

namespace FoodDelivery.Services.Customers.Customers.Data.Repositories.Mongo;

public class CustomerReadRepository(CustomersReadDbContext context, ISieveProcessor sieveProcessor)
    : MongoRepositoryBase<CustomersReadDbContext, CustomerReadModel, Guid>(context, sieveProcessor),
        ICustomerReadRepository;
