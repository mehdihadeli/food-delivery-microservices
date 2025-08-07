using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;
using MongoDB.Driver;

namespace FoodDelivery.Services.Customers.Shared.Data;

public class CustomersReadDbContext : MongoDbContext
{
    public CustomersReadDbContext(IMongoClient mongoClient, IMongoDatabase mongoDatabase)
        : base(mongoClient, mongoDatabase)
    {
        RestockSubscriptions = GetCollection<RestockSubscriptionReadModel>();
        Customers = GetCollection<CustomerReadModel>();
    }

    public IMongoCollection<RestockSubscriptionReadModel> RestockSubscriptions { get; }
    public IMongoCollection<CustomerReadModel> Customers { get; }
}
