using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;

namespace FoodDelivery.Services.Customers.Shared.Contracts;

public interface IRestockSubscriptionReadRepository : IRepository<RestockSubscriptionReadModel, Guid> { }
