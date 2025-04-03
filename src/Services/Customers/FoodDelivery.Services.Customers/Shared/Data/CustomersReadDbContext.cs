using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FoodDelivery.Services.Customers.Shared.Data;

public class CustomersReadDbContext : MongoDbContext
{
    public CustomersReadDbContext(IMongoClient mongoClient, IOptions<MongoOptions> options)
        : base(mongoClient, options)
    {
        RestockSubscriptions = GetCollection<RestockSubscriptionReadModel>();
        Customers = GetCollection<CustomerReadModel>();
    }

    public IMongoCollection<RestockSubscriptionReadModel> RestockSubscriptions { get; }
    public IMongoCollection<CustomerReadModel> Customers { get; }
}
