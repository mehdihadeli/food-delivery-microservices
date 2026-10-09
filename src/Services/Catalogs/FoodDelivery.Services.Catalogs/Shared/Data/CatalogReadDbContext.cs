using FoodDelivery.Services.Catalogs.Products.Models.Read;
using Humanizer;
using MongoDB.Driver;
using Squirrel.Persistence.Mongo;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CatalogReadDbContext : MongoDbContext
{
    public CatalogReadDbContext(IMongoClient mongoClient, IMongoDatabase mongoDatabase)
        : base(mongoClient, mongoDatabase)
    {
        Products = GetCollection<ProductReadModel>(nameof(Products).Underscore());
    }

    public IMongoCollection<ProductReadModel> Products { get; }
}
