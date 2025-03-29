using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Orders.Orders.Models.Reads;
using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FoodDelivery.Services.Orders.Shared.Data;

public class OrderReadDbContext : MongoDbContext
{
    public OrderReadDbContext(IMongoClient mongoClient, IOptions<MongoOptions> options)
        : base(mongoClient, options)
    {
        Orders = GetCollection<OrderReadModel>(nameof(Orders).Underscore());
    }

    public IMongoCollection<OrderReadModel> Orders { get; }
}
