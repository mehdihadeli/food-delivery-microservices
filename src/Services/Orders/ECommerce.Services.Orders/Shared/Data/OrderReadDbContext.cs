using BuildingBlocks.Persistence.Mongo;
using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ECommerce.Services.Orders.Orders.Models.Reads;

namespace ECommerce.Services.Orders.Shared.Data;

public class OrderReadDbContext : MongoDbContext
{
    public OrderReadDbContext(IOptions<MongoOptions> options) : base(options.Value)
    {
        Orders = GetCollection<OrderReadModel>(nameof(Orders).Underscore());
    }

    public IMongoCollection<OrderReadModel> Orders { get; }
}
