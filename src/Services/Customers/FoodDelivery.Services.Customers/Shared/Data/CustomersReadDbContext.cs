using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FoodDelivery.Services.Customers.Shared.Data;

public class CustomersReadDbContext : MongoDbContext
{
    public CustomersReadDbContext(IOptions<MongoOptions> options)
        : base(options.Value)
    {
        RestockSubscriptions = GetCollection<RestockSubscription>();
        Customers = GetCollection<Customer>();
    }

    public IMongoCollection<RestockSubscription> RestockSubscriptions { get; }
    public IMongoCollection<Customer> Customers { get; }
}
