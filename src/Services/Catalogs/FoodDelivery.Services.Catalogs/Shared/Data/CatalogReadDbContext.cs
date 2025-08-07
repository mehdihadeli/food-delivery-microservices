using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Services.Catalogs.Products.Models.Read;
using Humanizer;
using MongoDB.Driver;

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
